using System.Text.Json;
using ArixBack.Models;

namespace ArixBack.Services
{
    public class MatchEndService(
        EloService eloService,
        MatchLogService matchLogService,
        PlayerService playerService,
        WebsocketManager wsManager,
        MatchSessionStore sessionStore)
    {
        public async Task EndMatch(MatchSession session, string winnerId, string loserId)
        {
            if (Interlocked.CompareExchange(ref session.EndedFlag, 1, 0) != 0) return;
            session.BleedCts.Cancel();

            var winner = session.GetPlayer(winnerId)!;
            var loser = session.GetPlayer(loserId)!;

            var (newWinnerElo, newLoserElo) = eloService.Calculate(winner.Elo, loser.Elo);
            int winnerEloChange = newWinnerElo - winner.Elo;
            int loserEloChange = newLoserElo - loser.Elo;

            var winnerPlayer = await playerService.GetPlayerFromId(winnerId);
            var loserPlayer = await playerService.GetPlayerFromId(loserId);
            if (winnerPlayer?.Id != null) { winnerPlayer.Elo = newWinnerElo; await playerService.UpdatePlayer(winnerPlayer.Id, winnerPlayer); }
            if (loserPlayer?.Id != null) { loserPlayer.Elo = newLoserElo; await playerService.UpdatePlayer(loserPlayer.Id, loserPlayer); }

            session.Actions.Add(new MatchAction(DateTime.UtcNow, winnerId, "game_over", JsonSerializer.Serialize(new { winnerId })));

            var log = new MatchLog
            {
                Player1Id = session.Player1.PlayerId,
                Player2Id = session.Player2.PlayerId,
                StartedAt = session.StartedAt,
                EndedAt = DateTime.UtcNow,
                WinnerId = winnerId,
                Actions = session.Actions.ToList()
            };
            await matchLogService.SaveLog(log);

            await wsManager.SendToPlayer(winnerId, new { type = "game_over", won = true, eloChange = winnerEloChange, log = session.Actions });
            await wsManager.SendToPlayer(loserId, new { type = "game_over", won = false, eloChange = loserEloChange, log = session.Actions });

            sessionStore.RemoveSession(session.SessionId);
        }
    }
}
