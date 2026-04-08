using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ArixBack.Services
{
    public class MatchmakingBackgroundWorker : BackgroundService
    {
        private readonly MatchmakingService _matchmakingService;
        private readonly MatchSessionManager _matchSessionManager;
        private readonly ILogger<MatchmakingBackgroundWorker> _logger;

        public MatchmakingBackgroundWorker(
            MatchmakingService matchmakingService,
            MatchSessionManager matchSessionManager,
            ILogger<MatchmakingBackgroundWorker> logger)
        {
            _matchmakingService = matchmakingService;
            _matchSessionManager = matchSessionManager;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Matchmaking Background Worker is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DoMatchmaking();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during matchmaking.");
                }

                await Task.Delay(1000, stoppingToken);
            }

            _logger.LogInformation("Matchmaking Background Worker is stopping.");
        }

        private async Task DoMatchmaking()
        {
            var queue = _matchmakingService.GetQueue();
            _logger.LogInformation($"Matchmaking scan: {queue.Count} players in queue.");
            if (queue.Count < 2) return;

            var matchedPlayerIds = new HashSet<string>();

            for (int i = 0; i < queue.Count; i++)
            {
                var entryA = queue[i];
                if (matchedPlayerIds.Contains(entryA.Player.Id)) continue;

                for (int j = i + 1; j < queue.Count; j++)
                {
                    var entryB = queue[j];
                    if (matchedPlayerIds.Contains(entryB.Player.Id)) continue;
                    if (entryA.Player.Id == entryB.Player.Id) continue;

                    if (CanMatch(entryA, entryB))
                    {
                        _logger.LogInformation($"Match found: {entryA.Player.Username} vs {entryB.Player.Username}");

                        matchedPlayerIds.Add(entryA.Player.Id);
                        matchedPlayerIds.Add(entryB.Player.Id);

                        _matchmakingService.Dequeue(entryA.Player.Id);
                        _matchmakingService.Dequeue(entryB.Player.Id);

                        await _matchSessionManager.CreateSession(entryA, entryB);
                        break;
                    }
                }
            }
        }

        private bool CanMatch(PlayerQueueEntry a, PlayerQueueEntry b)
        {
            var waitTimeA = (DateTime.UtcNow - a.JoinedAt).TotalSeconds;
            var waitTimeB = (DateTime.UtcNow - b.JoinedAt).TotalSeconds;

            var rangeA = 200 + (5 * waitTimeA);
            var rangeB = 200 + (5 * waitTimeB);
            var maxAllowedDiff = Math.Max(rangeA, rangeB);

            var eloDiff = Math.Abs(a.Player.Elo - b.Player.Elo);

            return eloDiff <= maxAllowedDiff;
        }
    }
}
