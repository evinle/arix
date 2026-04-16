namespace ArixBack.Services.Questions
{
    public class AddSubtractTier : IQuestionTier
    {
        private static readonly Random _rng = Random.Shared;

        public Question Generate()
        {
            int a = _rng.Next(1, 21), b = _rng.Next(1, 21);
            bool add = _rng.Next(2) == 0;
            string text = add ? $"{a} + {b}" : $"{a + b} - {b}";
            string answer = add ? (a + b).ToString() : a.ToString();
            return new Question(Guid.NewGuid().ToString(), text, answer);
        }

        public bool Validate(Question q, string answer) => q.Answer == answer.Trim();
    }
}
