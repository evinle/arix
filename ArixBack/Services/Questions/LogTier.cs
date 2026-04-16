namespace ArixBack.Services.Questions
{
    public class LogTier : IQuestionTier
    {
        private static readonly Random _rng = Random.Shared;
        private static readonly int[] Bases = [2, 3, 5, 10];

        public Question Generate()
        {
            int b = Bases[_rng.Next(Bases.Length)];
            int exp = _rng.Next(1, 5); // result is 1–4
            int x = (int)Math.Pow(b, exp);
            string text = $"log_{b}({x})";
            return new Question(Guid.NewGuid().ToString(), text, exp.ToString());
        }

        public bool Validate(Question q, string answer) => q.Answer == answer.Trim();
    }
}
