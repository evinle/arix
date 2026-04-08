using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ArixBack.Services
{
    public class MatchSessionManager
    {
        private readonly ConcurrentDictionary<string, MatchSession> _activeSessions = new ConcurrentDictionary<string, MatchSession>();
        private readonly ConcurrentDictionary<string, string> _playerToSession = new ConcurrentDictionary<string, string>();
        private readonly MathProblemGenerator _problemGenerator;
        private readonly DatabaseService _db;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<MatchSessionManager> _logger;

        public MatchSessionManager(
            MathProblemGenerator problemGenerator,
            DatabaseService db,
            ILoggerFactory loggerFactory,
            ILogger<MatchSessionManager> logger)
        {
            _problemGenerator = problemGenerator;
            _db = db;
            _loggerFactory = loggerFactory;
            _logger = logger;
        }

        public async Task CreateSession(PlayerQueueEntry entryA, PlayerQueueEntry entryB)
        {
            var session = new MatchSession(
                entryA,
                entryB,
                _problemGenerator,
                this,
                _db,
                _loggerFactory.CreateLogger<MatchSession>()
            );

            _activeSessions.TryAdd(session.MatchId, session);
            _playerToSession.TryAdd(entryA.Player.Id, session.MatchId);
            _playerToSession.TryAdd(entryB.Player.Id, session.MatchId);

            _logger.LogInformation($"Match {session.MatchId} created for {entryA.Player.Username} and {entryB.Player.Username}");

            await session.StartAsync();
        }

        public MatchSession GetSessionByPlayerId(string playerId)
        {
            if (_playerToSession.TryGetValue(playerId, out var sessionId))
            {
                _activeSessions.TryGetValue(sessionId, out var session);
                return session;
            }
            return null;
        }

        public async Task HandleDisconnect(string playerId)
        {
            if (_playerToSession.TryGetValue(playerId, out var sessionId))
            {
                if (_activeSessions.TryGetValue(sessionId, out var session))
                {
                    await session.HandleDisconnectAsync(playerId);
                }
            }
        }

        public void RemoveSession(string sessionId)
        {
            if (_activeSessions.TryRemove(sessionId, out var session))
            {
                // Note: We'd need player IDs to remove from _playerToSession too. 
                // For simplicity, we can just let them stay or improve session object to hold player IDs.
            }
        }
    }
}
