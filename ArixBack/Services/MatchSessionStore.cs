using System.Collections.Concurrent;
using ArixBack.Models;
using ArixBack.Services.Questions;

namespace ArixBack.Services
{
    public class PlayerMatchState
    {
        public string PlayerId { get; set; } = "";
        public string PlayerName { get; set; } = "";
        public ClassType ClassType { get; set; }
        public int Hp { get; set; } = 100;
        public double WeaponDamageModifier { get; set; } = 1.0;
        public double ArmorDamageReductionModifier { get; set; } = 1.0;
        public int SkillTier { get; set; }
        public Question? CurrentQuestion { get; set; }
        public int ChargePoints { get; set; }
        public int BleedStacks { get; set; }
        public int BleedTicksRemaining { get; set; }
        public int CursedQuestionsRemaining { get; set; }
        public int CorrectStreak { get; set; }
        public int Elo { get; set; } = 1000;
    }

    public class MatchSession
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public PlayerMatchState Player1 { get; set; } = new();
        public PlayerMatchState Player2 { get; set; } = new();
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public List<MatchAction> Actions { get; set; } = new();
        public bool Ended { get; set; }

        public PlayerMatchState? GetPlayer(string playerId) =>
            Player1.PlayerId == playerId ? Player1 : Player2.PlayerId == playerId ? Player2 : null;

        public PlayerMatchState? GetOpponent(string playerId) =>
            Player1.PlayerId == playerId ? Player2 : Player2.PlayerId == playerId ? Player1 : null;
    }

    public class MatchSessionStore
    {
        private readonly ConcurrentDictionary<string, MatchSession> _sessions = new();
        // playerId -> sessionId
        private readonly ConcurrentDictionary<string, string> _playerToSession = new();

        public void AddSession(MatchSession session)
        {
            _sessions[session.SessionId] = session;
            _playerToSession[session.Player1.PlayerId] = session.SessionId;
            _playerToSession[session.Player2.PlayerId] = session.SessionId;
        }

        public MatchSession? GetSessionByPlayer(string playerId) =>
            _playerToSession.TryGetValue(playerId, out var sid) && _sessions.TryGetValue(sid, out var s) ? s : null;

        public void RemoveSession(string sessionId)
        {
            if (_sessions.TryRemove(sessionId, out var s))
            {
                _playerToSession.TryRemove(s.Player1.PlayerId, out _);
                _playerToSession.TryRemove(s.Player2.PlayerId, out _);
            }
        }
    }
}
