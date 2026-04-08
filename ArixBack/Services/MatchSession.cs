using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ArixBack.Models;
using ArixBack.Services;

using Microsoft.Extensions.Logging;

namespace ArixBack.Services
{
    public class MatchSession
    {
        private readonly string _matchId;
        private readonly PlayerQueueEntry _playerA;
        private readonly PlayerQueueEntry _playerB;
        private readonly MathProblemGenerator _problemGenerator;
        private readonly MatchSessionManager _sessionManager;
        private readonly DatabaseService _db;
        private readonly ILogger _logger;

        private int _hpA = 100;
        private int _hpB = 100;
        private MathProblem _currentProblemA;
        private MathProblem _currentProblemB;
        private readonly List<MatchEvent> _eventLog = new List<MatchEvent>();
        private readonly DateTime _startTime;
        private bool _isTerminated = false;

        public string MatchId => _matchId;

        public MatchSession(
            PlayerQueueEntry a,
            PlayerQueueEntry b,
            MathProblemGenerator problemGenerator,
            MatchSessionManager sessionManager,
            DatabaseService db,
            ILogger logger)
        {
            _matchId = Guid.NewGuid().ToString();
            _playerA = a;
            _playerB = b;
            _problemGenerator = problemGenerator;
            _sessionManager = sessionManager;
            _db = db;
            _logger = logger;
            _startTime = DateTime.UtcNow;

            LogEvent("MATCH_START", new { playerA = a.Player.Username, playerB = b.Player.Username });
        }

        public async Task StartAsync()
        {
            _currentProblemA = GenerateProblem(_playerA.Player);
            _currentProblemB = GenerateProblem(_playerB.Player);

            await BroadcastAsync("MATCH_FOUND", new
            {
                matchId = _matchId,
                playerA = new { username = _playerA.Player.Username, elo = _playerA.Player.Elo },
                playerB = new { username = _playerB.Player.Username, elo = _playerB.Player.Elo },
                initialHP = 100
            });

            await SendToPlayerAsync(_playerA, "NEW_PROBLEM", new { text = _currentProblemA.Text });
            await SendToPlayerAsync(_playerB, "NEW_PROBLEM", new { text = _currentProblemB.Text });
        }

        public async Task HandleMessageAsync(string playerId, string messageType, JsonElement payload)
        {
            if (_isTerminated) return;

            if (messageType == "SUBMIT_ANSWER")
            {
                string answer = payload.GetProperty("answer").GetString();
                await ProcessAnswerAsync(playerId, answer);
            }
        }

        public async Task HandleDisconnectAsync(string disconnectedPlayerId)
        {
            if (_isTerminated) return;

            string winnerId = disconnectedPlayerId == _playerA.Player.Id ? _playerB.Player.Id : _playerA.Player.Id;
            LogEvent("DISCONNECT", new { disconnectedPlayerId });
            await TerminateAsync(winnerId);
        }

        private async Task ProcessAnswerAsync(string playerId, string answer)
        {
            bool isPlayerA = playerId == _playerA.Player.Id;
            var player = isPlayerA ? _playerA : _playerB;
            var opponent = isPlayerA ? _playerB : _playerA;
            var currentProblem = isPlayerA ? _currentProblemA : _currentProblemB;

            LogEvent("SUBMIT_ANSWER", new { answer, correct = (answer == currentProblem.Answer) }, playerId);

            if (answer == currentProblem.Answer)
            {
                // Correct answer - Calculate damage
                int damage = CalculateDamage(player.Player, currentProblem);
                if (isPlayerA) _hpB -= damage; else _hpA -= damage;

                LogEvent("DAMAGE_DEALT", new { damage, targetId = opponent.Player.Id }, playerId);

                await BroadcastAsync("BATTLE_UPDATE", new
                {
                    attackerId = playerId,
                    damage,
                    hpA = _hpA,
                    hpB = _hpB
                });

                if (_hpA <= 0 || _hpB <= 0)
                {
                    await TerminateAsync(_hpA <= 0 ? _playerB.Player.Id : _playerA.Player.Id);
                    return;
                }

                // Generate next problem
                var nextProblem = GenerateProblem(player.Player);
                if (isPlayerA) _currentProblemA = nextProblem; else _currentProblemB = nextProblem;
                await SendToPlayerAsync(player, "NEW_PROBLEM", new { text = nextProblem.Text });
            }
            else
            {
                // Incorrect answer - Penalty? (e.g., 1s lockout)
                await SendToPlayerAsync(player, "INCORRECT_ANSWER", new { message = "Try again!" });
            }
        }

        private MathProblem GenerateProblem(Player player)
        {
            // Simple tiering for now. Tier 1 (Add/Sub), Tier 2 (Mul/Div), Tier 3 (Sqrt/Exp)
            // Attributes could increase tier or damage.
            int tier = 1;
            if (player.Elo > 1500) tier = 2;
            if (player.Elo > 2000) tier = 3;

            var prob = _problemGenerator.Generate(tier);
            LogEvent("GENERATE_PROB", new { text = prob.Text, tier }, player.Id);
            return prob;
        }

        private int CalculateDamage(Player player, MathProblem problem)
        {
            // Base damage + modifiers
            int damage = 10;
            // Example attribute logic
            // if (player.Class == "Wizard") damage += 5;
            return damage;
        }

        private async Task TerminateAsync(string winnerId)
        {
            _isTerminated = true;
            DateTime endTime = DateTime.UtcNow;
            LogEvent("MATCH_TERMINATED", new { winnerId });

            await BroadcastAsync("MATCH_TERMINATED", new
            {
                winnerId,
                finalHP = new { hpA = _hpA, hpB = _hpB }
            });

            // Flush to DB
            var matchRecord = new Match
            {
                StartTime = _startTime,
                EndTime = endTime,
                WinnerId = winnerId,
                Players = new List<MatchPlayerInfo>
                {
                    new MatchPlayerInfo { PlayerId = _playerA.Player.Id, Username = _playerA.Player.Username, EloBefore = _playerA.Player.Elo },
                    new MatchPlayerInfo { PlayerId = _playerB.Player.Id, Username = _playerB.Player.Username, EloBefore = _playerB.Player.Elo }
                },
                EventLog = _eventLog
            };

            await _db.GetMatchCollection().InsertOneAsync(matchRecord);
            _logger.LogInformation($"Match {_matchId} persisted to MongoDB.");
        }

        private void LogEvent(string type, object data = null, string playerId = null)
        {
            _eventLog.Add(new MatchEvent
            {
                Milliseconds = (long)(DateTime.UtcNow - _startTime).TotalMilliseconds,
                Type = type,
                Data = data,
                PlayerId = playerId
            });
        }

        private async Task BroadcastAsync(string type, object payload)
        {
            await SendToPlayerAsync(_playerA, type, payload);
            await SendToPlayerAsync(_playerB, type, payload);
        }

        private async Task SendToPlayerAsync(PlayerQueueEntry player, string type, object payload)
        {
            if (player.Socket.State != WebSocketState.Open) return;

            var json = JsonSerializer.Serialize(new { type, payload });
            var bytes = Encoding.UTF8.GetBytes(json);
            await player.Socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
