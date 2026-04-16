using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using ArixBack.Models;
using ArixBack.Services;
using ArixBack.Services.Questions;
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
        private readonly MatchEndService _matchEndService;
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
            MatchEndService matchEndService,
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
            _matchEndService = matchEndService;
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
                var session = _sessionStore.GetSessionByPlayer(playerId);
                if (session != null && !session.Ended)
                {
                    var opponent = session.GetOpponent(playerId);
                    var self = session.GetPlayer(playerId);
                    if (opponent != null && self != null)
                        await _matchEndService.EndMatch(session, opponent.PlayerId, self.PlayerId);
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

            await session.Lock.WaitAsync();
            bool correct;
            bool cursedWrong = false;
            int baseDamage = 0;
            EffectResult? effectResult = null;
            EffectResult? hitResult = null;
            string? effect = null;
            int selfHp = 0, opponentHp = 0, damageDealt = 0, damageTaken = 0;
            Question? nextQuestion = null;
            bool gameOver = false;
            try
            {
                if (session.Ended) return;
                if (self.CurrentQuestion == null || self.CurrentQuestion.Id != questionId) return;

                var tier = _questionService.GetTier(self.CursedQuestionsRemaining > 0 ? Math.Min(self.SkillTier + 1, 4) : self.SkillTier);
                correct = tier.Validate(self.CurrentQuestion, value);

                if (!correct)
                {
                    if (self.CursedQuestionsRemaining > 0)
                    {
                        self.CursedQuestionsRemaining--;
                        self.Hp -= 15;
                        session.Actions.Add(new MatchAction(DateTime.UtcNow, playerId, "wrong_cursed", null));
                        cursedWrong = true;
                    }
                    self.CorrectStreak = 0;
                    self.CurrentQuestion = _questionService.GetTier(self.SkillTier).Generate();
                    nextQuestion = self.CurrentQuestion;
                    selfHp = self.Hp;
                    opponentHp = opponent.Hp;
                }
                else
                {
                    if (self.CursedQuestionsRemaining > 0)
                        self.CursedQuestionsRemaining--;

                    baseDamage = (int)(20 * self.WeaponDamageModifier);
                    effectResult = _classEffects.ApplyOnCorrectAnswer(self, opponent, baseDamage);
                    hitResult = _classEffects.ApplyOnHit(opponent, effectResult.DamageToOpponent);

                    opponent.Hp -= hitResult.DamageToSelf;
                    self.Hp -= hitResult.DamageToOpponent;
                    self.Hp += effectResult.HealSelf;

                    effect = effectResult.EffectMessage ?? hitResult.EffectMessage;
                    damageDealt = hitResult.DamageToSelf;
                    damageTaken = hitResult.DamageToOpponent;
                    selfHp = self.Hp;
                    opponentHp = opponent.Hp;

                    session.Actions.Add(new MatchAction(DateTime.UtcNow, playerId, "correct_answer",
                        JsonSerializer.Serialize(new { damage = hitResult.DamageToSelf, effect })));

                    if (self.Hp <= 0 || opponent.Hp <= 0)
                        gameOver = true;
                    else
                    {
                        self.CurrentQuestion = _questionService.GetTier(self.SkillTier).Generate();
                        nextQuestion = self.CurrentQuestion;
                    }
                }
            }
            finally { session.Lock.Release(); }

            if (!correct)
            {
                if (cursedWrong)
                {
                    if (self.CursedQuestionsRemaining == 0)
                        await _wsManager.SendToPlayer(playerId, new { type = "curse_removed" });
                    if (selfHp <= 0 || opponentHp <= 0)
                    {
                        await CheckGameOver(session, playerId, opponent.PlayerId);
                        return;
                    }
                }
                await _wsManager.SendToPlayer(playerId, new { type = "question", id = nextQuestion!.Id, text = nextQuestion.Text });
                return;
            }

            if (effectResult!.EffectMessage == "curse_applied")
                await _wsManager.SendToPlayer(opponent.PlayerId, new { type = "curse_applied", questionsAffected = 3 });

            await SendHitBoth(session, selfHp, opponentHp, self.PlayerId, opponent.PlayerId, damageDealt, damageTaken, effect);

            if (gameOver)
            {
                await CheckGameOver(session, playerId, opponent.PlayerId);
                return;
            }

            await _wsManager.SendToPlayer(playerId, new { type = "question", id = nextQuestion!.Id, text = nextQuestion.Text });
        }

        private async Task HandleSkip(string playerId)
        {
            var session = _sessionStore.GetSessionByPlayer(playerId);
            if (session == null || session.Ended) return;

            var self = session.GetPlayer(playerId)!;
            var opponent = session.GetOpponent(playerId)!;

            int selfHp, opponentHp;
            await session.Lock.WaitAsync();
            try
            {
                self.Hp -= 10;
                self.CorrectStreak = 0;
                session.Actions.Add(new MatchAction(DateTime.UtcNow, playerId, "skip", null));
                selfHp = self.Hp;
                opponentHp = opponent.Hp;
            }
            finally { session.Lock.Release(); }

            await SendHitBoth(session, selfHp, opponentHp, self.PlayerId, opponent.PlayerId, 0, 10, null);

            if (await CheckGameOver(session, playerId, opponent.PlayerId)) return;

            await session.Lock.WaitAsync();
            Question nextQuestion;
            try
            {
                self.CurrentQuestion = _questionService.GetTier(self.SkillTier).Generate();
                nextQuestion = self.CurrentQuestion!;
            }
            finally { session.Lock.Release(); }

            await _wsManager.SendToPlayer(playerId, new { type = "question", id = nextQuestion.Id, text = nextQuestion.Text });
        }

        private async Task HandleReleaseCharge(string playerId)
        {
            var session = _sessionStore.GetSessionByPlayer(playerId);
            if (session == null || session.Ended) return;

            var self = session.GetPlayer(playerId)!;
            var opponent = session.GetOpponent(playerId)!;

            if (self.ClassType != ArixBack.Models.ClassType.Berserker) return;

            int charge, damageDealt, damageTaken;
            string? effect;
            await session.Lock.WaitAsync();
            try
            {
                charge = _classEffects.ReleaseCharge(self);
                var hitResult = _classEffects.ApplyOnHit(opponent, charge);
                opponent.Hp -= hitResult.DamageToSelf;
                self.Hp -= hitResult.DamageToOpponent;
                damageDealt = hitResult.DamageToSelf;
                damageTaken = hitResult.DamageToOpponent;
                effect = hitResult.EffectMessage;
                session.Actions.Add(new MatchAction(DateTime.UtcNow, playerId, "charge_release",
                    JsonSerializer.Serialize(new { damage = charge })));
            }
            finally { session.Lock.Release(); }

            await SendHitBoth(session, self.Hp, opponent.Hp, self.PlayerId, opponent.PlayerId, damageDealt, damageTaken, effect);

            await CheckGameOver(session, playerId, opponent.PlayerId);
        }

        private async Task SendHitBoth(MatchSession session, int selfHp, int opponentHp, string selfId, string opponentId, int damageDealt, int damageTaken, string? effect)
        {
            await _wsManager.SendToPlayer(selfId, new
            {
                type = "hit",
                yourHp = selfHp,
                opponentHp,
                damageDealt,
                damageTaken,
                effect
            });
            await _wsManager.SendToPlayer(opponentId, new
            {
                type = "hit",
                yourHp = opponentHp,
                opponentHp = selfHp,
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
                await _matchEndService.EndMatch(session, defenderId, attackerId);
                return true;
            }
            if (opponent.Hp <= 0)
            {
                await _matchEndService.EndMatch(session, attackerId, defenderId);
                return true;
            }
            if (self.Hp <= 0)
            {
                await _matchEndService.EndMatch(session, defenderId, attackerId);
                return true;
            }
            return false;
        }

        [HttpGet("GetAllConnections")]
        public IEnumerable<System.Net.WebSockets.WebSocket> GetAllConnections() => _wsManager.GetAllConnections();
    }
}
