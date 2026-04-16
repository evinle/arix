using ArixBack.Services.Questions;

namespace ArixBack.Services
{
    public class QuestionService
    {
        private readonly IQuestionTier[] _tiers =
        [
            new AddSubtractTier(),
            new MultiplyDivideTier(),
            new ExponentRootTier(),
            new LogTier(),
            new EquationTier(),
        ];

        public IQuestionTier GetTier(int tier) => _tiers[Math.Clamp(tier, 0, _tiers.Length - 1)];
    }
}
