namespace ArixBack.Services.Questions
{
    public class MultiplyDivideTier : IQuestionTier
    {
        private static readonly Random _rng = Random.Shared;

        public Question Generate()
        {
            int a = _rng.Next(1, 13), b = _rng.Next(1, 13);
            bool multiply = _rng.Next(2) == 0;
            string text = multiply ? $"{a} × {b}" : $"{a * b} ÷ {b}";
            string answer = multiply ? (a * b).ToString() : a.ToString();
            return new Question(Guid.NewGuid().ToString(), text, answer);
        }

        public bool Validate(Question q, string answer) => q.Answer == answer.Trim();
    }
}
