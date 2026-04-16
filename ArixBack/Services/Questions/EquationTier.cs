namespace ArixBack.Services.Questions
{
    public class EquationTier : IQuestionTier
    {
        private static readonly Random _rng = Random.Shared;

        public Question Generate()
        {
            // ax + b = c, integer solution x
            int a = _rng.Next(1, 11);
            int x = _rng.Next(-10, 11);
            int b = _rng.Next(-20, 21);
            int c = a * x + b;
            string text = b >= 0 ? $"{a}x + {b} = {c}" : $"{a}x - {Math.Abs(b)} = {c}";
            return new Question(Guid.NewGuid().ToString(), text, x.ToString());
        }

        public bool Validate(Question q, string answer) => q.Answer == answer.Trim();
    }
}
