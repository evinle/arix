namespace ArixBack.Services.Questions
{
    public class ExponentRootTier : IQuestionTier
    {
        private static readonly Random _rng = Random.Shared;

        public Question Generate()
        {
            int a = _rng.Next(2, 11);
            int b = _rng.Next(2) == 0 ? 2 : 3;
            bool exponent = _rng.Next(2) == 0;
            int power = (int)Math.Pow(a, b);
            string text = exponent ? $"{a}^{b}" : (b == 2 ? $"√{power}" : $"∛{power}");
            string answer = exponent ? power.ToString() : a.ToString();
            return new Question(Guid.NewGuid().ToString(), text, answer);
        }

        public bool Validate(Question q, string answer) => q.Answer == answer.Trim();
    }
}
