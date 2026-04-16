using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using ArixBack.Models;
using ArixBack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArixBack.Controllers
{
    [Authorize]
    [ApiController]
    [Route("Websocket")]
    public class Matchmaking : ControllerBase
    {
        private readonly ILogger<Matchmaking> _logger;
        private readonly WebsocketManager _wsManager;
        private readonly PlayerService _playerService;
        private readonly WeaponService _weaponService;
        private readonly ArmorService _armorService;
        private readonly MatchmakingQueue _queue;
        private readonly MatchSessionStore _sessionStore;
        private readonly ClassEffectService _classEffects;
        private readonly EloService _eloService;
        private readonly MatchLogService _matchLogService;
        private readonly QuestionService _questionService;

        public Matchmaking(
            ILogger<Matchmaking> logger,
            WebsocketManager wsManager,
            PlayerService playerService,
            WeaponService weaponService,
            ArmorService armorService,
            MatchmakingQueue queue,
            MatchSessionStore sessionStore,
            ClassEffectService classEffects,
            EloService eloService,
            MatchLogService matchLogService,
            QuestionService questionService)
        {
            _logger = logger;
            _wsManager = wsManager;
            _playerService = playerService;
            _weaponService = weaponService;
            _armorService = armorService;
            _queue = queue;
            _sessionStore = sessionStore;
            _classEffects = classEffects;
            _eloService = eloService;
            _matchLogService = matchLogService;
            _questionService = questionService;
        }

        [HttpGet("ws")]
        public async Task Matchmake()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = 400;
                return;
            }

            var userName = User.FindFirstValue(JwtRegisteredClaimNames.NameId);
            if (userName == null)
            {
                HttpContext.Response.StatusCode = 401;
                return;
            }

            var player = await _playerService.GetPlayerFromUsername(userName);
            if (player?.Id == null)
            {
                HttpContext.Response.StatusCode = 404;
                return;
            }

            using var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
            var playerId = player.Id;
            _wsManager.AddConnection(playerId, ws);

            try
            {
                await GameLoop(ws, player);
            }
            finally
            {
                _wsManager.RemoveConnection(playerId);
                // Handle disconnect: if in active match, opponent wins
                var session = _sessionStore.GetSessionByPlayer(playerId);
                if (session != null && !session.Ended)
                {
                    var opponent = session.GetOpponent(playerId);
                    var self = session.GetPlayer(playerId);
                    if (opponent != null && self != null)
                        await EndMatch(session, opponent.PlayerId, self.PlayerId);
                }
            }
        }

        private async Task GameLoop(WebSocket ws, Player player)
        {
            var playerId = player.Id!;
            var buffer = new byte[4096];

            while (ws.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;
                try
                {
                    result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
                catch
                {
                    break;
                }

                if (result.CloseStatus.HasValue)
                {
                    await ws.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                    break;
                }

                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                JsonDocument doc;
                try { doc = JsonDocument.Parse(json); }
                catch { continue; }

                using (doc)
                {
                    var type = doc.RootElement.GetProperty("type").GetString();
                    switch (type)
                    {
                        case "queue":
                            await HandleQueue(doc.RootElement, player);
                            break;
                        case "answer":
                            await HandleAnswer(doc.RootElement, playerId);
                            break;
                        case "skip":
                            await HandleSkip(playerId);
                            break;
                        case "release_charge":
                            await HandleReleaseCharge(playerId);
                            break;
                    }
                }
            }
        }

        private async Task HandleQueue(JsonElement msg, Player player)
        {
            var playerId = player.Id!;
            int skillTier = msg.TryGetProperty("skillTier", out var st) ? st.GetInt32() : 0;

            double weaponMod = 1.0;
            double armorMod = 1.0;

            if (player.EquippedWeaponId != null)
            {
                var weapon = await _weaponService.GetWeapon(player.EquippedWeaponId);
                if (weapon != null) weaponMod = weapon.DamageModifier;
            }
            if (player.EquippedArmorId != null)
            {
                var armor = await _armorService.GetArmor(player.EquippedArmorId);
                if (armor != null) armorMod = armor.DamageReductionModifier;
            }

            _queue.Enqueue(new QueueEntry
            {
                PlayerId = playerId,
                PlayerName = player.Username,
                Elo = player.Elo,
                SkillTier = skillTier,
                ClassType = player.ClassType,
                WeaponDamageModifier = weaponMod,
                ArmorDamageReductionModifier = armorMod,
            });

            await _wsManager.SendToPlayer(playerId, new { type = "waiting" });
        }

        private async Task HandleAnswer(JsonElement msg, string playerId)
        {
            var session = _sessionStore.GetSessionByPlayer(playerId);
            if (session == null || session.Ended) return;

            var self = session.GetPlayer(playerId)!;
            var opponent = session.GetOpponent(playerId)!;

            var questionId = msg.GetProperty("questionId").GetString();
            var value = msg.GetProperty("value").ToString();

            if (self.CurrentQuestion == null || self.CurrentQuestion.Id != questionId)
                return;

            var tier = _questionService.GetTier(self.CursedQuestionsRemaining > 0 ? Math.Min(self.SkillTier + 1, 4) : self.SkillTier);
            bool correct = tier.Validate(self.CurrentQuestion, value);

            if (!correct)
            {
                // Wrong answer on cursed question: lose 15 HP
                if (self.CursedQuestionsRemaining > 0)
                {
                    self.CursedQuestionsRemaining--;
                    self.Hp -= 15;
                    session.Actions.Add(new MatchAction(DateTime.UtcNow, playerId, "wrong_cursed", null));
                    if (self.CursedQuestionsRemaining == 0)
                        await _wsManager.SendToPlayer(playerId, new { type = "curse_removed" });
                    if (await CheckGameOver(session, playerId, opponent.PlayerId)) return;
                }
                // Assign new question
                self.CorrectStreak = 0;
                self.CurrentQuestion = _questionService.GetTier(self.SkillTier).Generate();
                await _wsManager.SendToPlayer(playerId, new { type = "question", id = self.CurrentQuestion.Id, text = self.CurrentQuestion.Text });
                return;
            }

            // Correct answer
            if (self.CursedQuestionsRemaining > 0)
                self.CursedQuestionsRemaining--;

            int baseDamage = (int)(20 * self.WeaponDamageModifier);
            var effectResult = _classEffects.ApplyOnCorrectAnswer(self, opponent, baseDamage);
            var hitResult = _classEffects.ApplyOnHit(opponent, effectResult.DamageToOpponent);

            opponent.Hp -= hitResult.DamageToSelf; // reduced damage to opponent
            self.Hp -= hitResult.DamageToOpponent; // juggernaut reflect
            self.Hp += effectResult.HealSelf;

            string? effect = effectResult.EffectMessage ?? hitResult.EffectMessage;

            session.Actions.Add(new MatchAction(DateTime.UtcNow, playerId, "correct_answer",
                JsonSerializer.Serialize(new { damage = hitResult.DamageToSelf, effect })));

            // Notify curse applied
            if (effectResult.EffectMessage == "curse_applied")
                await _wsManager.SendToPlayer(opponent.PlayerId, new { type = "curse_applied", questionsAffected = 3 });

            await SendHitBoth(session, self, opponent, hitResult.DamageToSelf, hitResult.DamageToOpponent, effect);

            if (await CheckGameOver(session, playerId, opponent.PlayerId)) return;

            // New question for answerer
            self.CurrentQuestion = _questionService.GetTier(self.SkillTier).Generate();
            await _wsManager.SendToPlayer(playerId, new { type = "question", id = self.CurrentQuestion.Id, text = self.CurrentQuestion.Text });
        }

        private async Task HandleSkip(string playerId)
        {
            var session = _sessionStore.GetSessionByPlayer(playerId);
            if (session == null || session.Ended) return;

            var self = session.GetPlayer(playerId)!;
            var opponent = session.GetOpponent(playerId)!;

            self.Hp -= 10;
            self.CorrectStreak = 0;
            session.Actions.Add(new MatchAction(DateTime.UtcNow, playerId, "skip", null));

            await SendHitBoth(session, self, opponent, 0, 10, null);

            if (await CheckGameOver(session, playerId, opponent.PlayerId)) return;

            self.CurrentQuestion = _questionService.GetTier(self.SkillTier).Generate();
            await _wsManager.SendToPlayer(playerId, new { type = "question", id = self.CurrentQuestion.Id, text = self.CurrentQuestion.Text });
        }

        private async Task HandleReleaseCharge(string playerId)
        {
            var session = _sessionStore.GetSessionByPlayer(playerId);
            if (session == null || session.Ended) return;

            var self = session.GetPlayer(playerId)!;
            var opponent = session.GetOpponent(playerId)!;

            if (self.ClassType != ArixBack.Models.ClassType.Berserker) return;

            int charge = _classEffects.ReleaseCharge(self);
            var hitResult = _classEffects.ApplyOnHit(opponent, charge);

            opponent.Hp -= hitResult.DamageToSelf;
            self.Hp -= hitResult.DamageToOpponent;

            session.Actions.Add(new MatchAction(DateTime.UtcNow, playerId, "charge_release",
                JsonSerializer.Serialize(new { damage = charge })));

            await SendHitBoth(session, self, opponent, hitResult.DamageToSelf, hitResult.DamageToOpponent, hitResult.EffectMessage);

            await CheckGameOver(session, playerId, opponent.PlayerId);
        }

        private async Task SendHitBoth(MatchSession session, PlayerMatchState self, PlayerMatchState opponent, int damageDealt, int damageTaken, string? effect)
        {
            await _wsManager.SendToPlayer(self.PlayerId, new
            {
                type = "hit",
                yourHp = self.Hp,
                opponentHp = opponent.Hp,
                damageDealt,
                damageTaken,
                effect
            });
            await _wsManager.SendToPlayer(opponent.PlayerId, new
            {
                type = "hit",
                yourHp = opponent.Hp,
                opponentHp = self.Hp,
                damageDealt = damageTaken,
                damageTaken = damageDealt,
                effect
            });
        }

        private async Task<bool> CheckGameOver(MatchSession session, string attackerId, string defenderId)
        {
            var self = session.GetPlayer(attackerId)!;
            var opponent = session.GetPlayer(defenderId)!;

            if (self.Hp <= 0 && opponent.Hp <= 0)
            {
                // Both dead — attacker loses (edge case)
                await EndMatch(session, defenderId, attackerId);
                return true;
            }
            if (opponent.Hp <= 0)
            {
                await EndMatch(session, attackerId, defenderId);
                return true;
            }
            if (self.Hp <= 0)
            {
                await EndMatch(session, defenderId, attackerId);
                return true;
            }
            return false;
        }

        private async Task EndMatch(MatchSession session, string winnerId, string loserId)
        {
            if (session.Ended) return;
            session.Ended = true;

            var winner = session.GetPlayer(winnerId)!;
            var loser = session.GetPlayer(loserId)!;

            var (newWinnerElo, newLoserElo) = _eloService.Calculate(winner.Elo, loser.Elo);
            int winnerEloChange = newWinnerElo - winner.Elo;
            int loserEloChange = newLoserElo - loser.Elo;

            // Update DB
            var winnerPlayer = await _playerService.GetPlayerFromId(winnerId);
            var loserPlayer = await _playerService.GetPlayerFromId(loserId);
            if (winnerPlayer?.Id != null) { winnerPlayer.Elo = newWinnerElo; await _playerService.UpdatePlayer(winnerPlayer.Id, winnerPlayer); }
            if (loserPlayer?.Id != null) { loserPlayer.Elo = newLoserElo; await _playerService.UpdatePlayer(loserPlayer.Id, loserPlayer); }

            session.Actions.Add(new MatchAction(DateTime.UtcNow, winnerId, "game_over", JsonSerializer.Serialize(new { winnerId })));

            var log = new MatchLog
            {
                Player1Id = session.Player1.PlayerId,
                Player2Id = session.Player2.PlayerId,
                StartedAt = session.StartedAt,
                EndedAt = DateTime.UtcNow,
                WinnerId = winnerId,
                Actions = session.Actions
            };
            await _matchLogService.SaveLog(log);

            await _wsManager.SendToPlayer(winnerId, new { type = "game_over", won = true, eloChange = winnerEloChange, log = session.Actions });
            await _wsManager.SendToPlayer(loserId, new { type = "game_over", won = false, eloChange = loserEloChange, log = session.Actions });

            _sessionStore.RemoveSession(session.SessionId);
        }

        [HttpGet("GetAllConnections")]
        public IEnumerable<System.Net.WebSockets.WebSocket> GetAllConnections() => _wsManager.GetAllConnections();
    }
}
