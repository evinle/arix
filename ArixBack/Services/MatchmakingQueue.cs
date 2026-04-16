using System.Collections.Concurrent;
using ArixBack.Models;
using ArixBack.Services.Questions;

namespace ArixBack.Services
{
    public class QueueEntry
    {
        public string PlayerId { get; set; } = "";
        public string PlayerName { get; set; } = "";
        public int Elo { get; set; }
        public int SkillTier { get; set; }
        public ClassType ClassType { get; set; }
        public double WeaponDamageModifier { get; set; } = 1.0;
        public double ArmorDamageReductionModifier { get; set; } = 1.0;
        public DateTime EnqueuedAt { get; set; } = DateTime.UtcNow;
    }

    public class MatchmakingQueue
    {
        private readonly ConcurrentQueue<QueueEntry> _queue = new();
        private readonly MatchSessionStore _sessionStore;
        private readonly WebsocketManager _wsManager;
        private readonly QuestionService _questionService;
        private readonly ClassEffectService _classEffects;
        private readonly MatchEndService _matchEndService;

        public MatchmakingQueue(
            MatchSessionStore sessionStore,
            WebsocketManager wsManager,
            QuestionService questionService,
            ClassEffectService classEffects,
            MatchEndService matchEndService)
        {
            _sessionStore = sessionStore;
            _wsManager = wsManager;
            _questionService = questionService;
            _classEffects = classEffects;
            _matchEndService = matchEndService;
        }

        public void Enqueue(QueueEntry entry) => _queue.Enqueue(entry);

        public void StartBackground(CancellationToken ct)
        {
            Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    await Task.Delay(2000, ct);
                    TryPair();
                }
            }, ct);
        }

        private void TryPair()
        {
            var candidates = new List<QueueEntry>();
            while (_queue.TryDequeue(out var entry))
                candidates.Add(entry);

            var paired = new HashSet<int>();
            for (int i = 0; i < candidates.Count; i++)
            {
                if (paired.Contains(i)) continue;
                var a = candidates[i];
                for (int j = i + 1; j < candidates.Count; j++)
                {
                    if (paired.Contains(j)) continue;
                    var b = candidates[j];
                    double secondsA = (DateTime.UtcNow - a.EnqueuedAt).TotalSeconds;
                    double secondsB = (DateTime.UtcNow - b.EnqueuedAt).TotalSeconds;
                    double allowedGap = 100 + (Math.Min(secondsA, secondsB) / 10) * 50;
                    if (Math.Abs(a.Elo - b.Elo) <= allowedGap)
                    {
                        paired.Add(i);
                        paired.Add(j);
                        _ = StartMatch(a, b);
                        break;
                    }
                }
            }

            // Re-enqueue unmatched
            for (int i = 0; i < candidates.Count; i++)
                if (!paired.Contains(i))
                    _queue.Enqueue(candidates[i]);
        }

        private async Task StartMatch(QueueEntry a, QueueEntry b)
        {
            var p1 = ToMatchState(a);
            var p2 = ToMatchState(b);
            p1.CurrentQuestion = _questionService.GetTier(p1.SkillTier).Generate();
            p2.CurrentQuestion = _questionService.GetTier(p2.SkillTier).Generate();

            var session = new MatchSession { Player1 = p1, Player2 = p2 };
            _sessionStore.AddSession(session);

            await _wsManager.SendToPlayer(a.PlayerId, new
            {
                type = "match_start",
                opponentName = b.PlayerName,
                opponentClass = b.ClassType.ToString(),
                yourHp = p1.Hp,
                opponentHp = p2.Hp,
                question = new { id = p1.CurrentQuestion.Id, text = p1.CurrentQuestion.Text },
                skillTier = a.SkillTier
            });

            await _wsManager.SendToPlayer(b.PlayerId, new
            {
                type = "match_start",
                opponentName = a.PlayerName,
                opponentClass = a.ClassType.ToString(),
                yourHp = p2.Hp,
                opponentHp = p1.Hp,
                question = new { id = p2.CurrentQuestion.Id, text = p2.CurrentQuestion.Text },
                skillTier = b.SkillTier
            });

            _ = RunBleedLoop(session);
        }

        private async Task RunBleedLoop(MatchSession session)
        {
            var ct = session.BleedCts.Token;
            while (!session.Ended)
            {
                try { await Task.Delay(5000, ct); } catch (OperationCanceledException) { return; }
                if (session.Ended) break;

                foreach (var player in new[] { session.Player1, session.Player2 })
                {
                    await session.Lock.WaitAsync();
                    int damage;
                    int hp;
                    try
                    {
                        damage = _classEffects.TickBleed(player);
                        if (damage <= 0) continue;
                        player.Hp -= damage;
                        hp = player.Hp;
                    }
                    finally { session.Lock.Release(); }

                    session.Actions.Add(new MatchAction(DateTime.UtcNow, player.PlayerId, "bleed_tick", null));
                    await _wsManager.SendToPlayer(player.PlayerId, new { type = "bleed_tick", yourHp = hp, amount = damage });

                    if (hp <= 0 && !session.Ended)
                    {
                        var opponent = session.GetOpponent(player.PlayerId)!;
                        await _matchEndService.EndMatch(session, opponent.PlayerId, player.PlayerId);
                        return;
                    }
                }
            }
        }

        private static PlayerMatchState ToMatchState(QueueEntry e) => new()
        {
            PlayerId = e.PlayerId,
            PlayerName = e.PlayerName,
            ClassType = e.ClassType,
            Elo = e.Elo,
            SkillTier = e.SkillTier,
            WeaponDamageModifier = e.WeaponDamageModifier,
            ArmorDamageReductionModifier = e.ArmorDamageReductionModifier,
        };
    }
}
