namespace ArixBack.Services
{
    public class EloService
    {
        private const int K = 32;

        public (int newWinnerElo, int newLoserElo) Calculate(int winnerElo, int loserElo)
        {
            double expectedWinner = 1.0 / (1.0 + Math.Pow(10, (loserElo - winnerElo) / 400.0));
            double expectedLoser = 1.0 - expectedWinner;
            int newWinner = (int)Math.Round(winnerElo + K * (1 - expectedWinner));
            int newLoser = (int)Math.Round(loserElo + K * (0 - expectedLoser));
            return (newWinner, newLoser);
        }
    }
}
