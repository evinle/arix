using ArixBack.Services;
using ArixBack.Services.Questions;
using Xunit;

namespace ArixBack.Tests;

public class QuestionTierTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static void AssertGeneratesValidQuestion(IQuestionTier tier)
    {
        var q = tier.Generate();
        Assert.NotNull(q);
        Assert.False(string.IsNullOrWhiteSpace(q.Id));
        Assert.False(string.IsNullOrWhiteSpace(q.Text));
        Assert.False(string.IsNullOrWhiteSpace(q.Answer));
    }

    private static void AssertCorrectAnswerValidates(IQuestionTier tier)
    {
        // Run many times to cover random branches
        for (int i = 0; i < 50; i++)
        {
            var q = tier.Generate();
            Assert.True(tier.Validate(q, q.Answer), $"Correct answer '{q.Answer}' failed for '{q.Text}'");
        }
    }

    private static void AssertWrongAnswerFails(IQuestionTier tier)
    {
        var q = tier.Generate();
        Assert.False(tier.Validate(q, "WRONG_ANSWER_XYZ"));
    }

    private static void AssertValidateTrimsWhitespace(IQuestionTier tier)
    {
        var q = tier.Generate();
        Assert.True(tier.Validate(q, "  " + q.Answer + "  "));
    }

    // ── Tier 0: AddSubtract ───────────────────────────────────────────────

    [Fact] public void AddSubtract_GeneratesValidQuestion() => AssertGeneratesValidQuestion(new AddSubtractTier());
    [Fact] public void AddSubtract_CorrectAnswerValidates() => AssertCorrectAnswerValidates(new AddSubtractTier());
    [Fact] public void AddSubtract_WrongAnswerFails() => AssertWrongAnswerFails(new AddSubtractTier());
    [Fact] public void AddSubtract_ValidateTrimsWhitespace() => AssertValidateTrimsWhitespace(new AddSubtractTier());

    // ── Tier 1: MultiplyDivide ────────────────────────────────────────────

    [Fact] public void MultiplyDivide_GeneratesValidQuestion() => AssertGeneratesValidQuestion(new MultiplyDivideTier());
    [Fact] public void MultiplyDivide_CorrectAnswerValidates() => AssertCorrectAnswerValidates(new MultiplyDivideTier());
    [Fact] public void MultiplyDivide_WrongAnswerFails() => AssertWrongAnswerFails(new MultiplyDivideTier());
    [Fact] public void MultiplyDivide_ValidateTrimsWhitespace() => AssertValidateTrimsWhitespace(new MultiplyDivideTier());

    // ── Tier 2: ExponentRoot ──────────────────────────────────────────────

    [Fact] public void ExponentRoot_GeneratesValidQuestion() => AssertGeneratesValidQuestion(new ExponentRootTier());
    [Fact] public void ExponentRoot_CorrectAnswerValidates() => AssertCorrectAnswerValidates(new ExponentRootTier());
    [Fact] public void ExponentRoot_WrongAnswerFails() => AssertWrongAnswerFails(new ExponentRootTier());
    [Fact] public void ExponentRoot_ValidateTrimsWhitespace() => AssertValidateTrimsWhitespace(new ExponentRootTier());

    // ── Tier 3: Log ───────────────────────────────────────────────────────

    [Fact] public void Log_GeneratesValidQuestion() => AssertGeneratesValidQuestion(new LogTier());
    [Fact] public void Log_CorrectAnswerValidates() => AssertCorrectAnswerValidates(new LogTier());
    [Fact] public void Log_WrongAnswerFails() => AssertWrongAnswerFails(new LogTier());
    [Fact] public void Log_ValidateTrimsWhitespace() => AssertValidateTrimsWhitespace(new LogTier());

    // ── Tier 4: Equation ─────────────────────────────────────────────────

    [Fact] public void Equation_GeneratesValidQuestion() => AssertGeneratesValidQuestion(new EquationTier());
    [Fact] public void Equation_CorrectAnswerValidates() => AssertCorrectAnswerValidates(new EquationTier());
    [Fact] public void Equation_WrongAnswerFails() => AssertWrongAnswerFails(new EquationTier());
    [Fact] public void Equation_ValidateTrimsWhitespace() => AssertValidateTrimsWhitespace(new EquationTier());

    // ── QuestionService ───────────────────────────────────────────────────

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)]
    public void QuestionService_GetTier_ReturnsNonNull(int tier)
    {
        var svc = new QuestionService();
        Assert.NotNull(svc.GetTier(tier));
    }

    [Fact]
    public void QuestionService_GetTier_ClampsAboveMax()
    {
        var svc = new QuestionService();
        // tier 99 should clamp to 4 (EquationTier), not throw
        var t = svc.GetTier(99);
        Assert.NotNull(t);
        Assert.IsType<EquationTier>(t);
    }

    [Fact]
    public void QuestionService_GetTier_ClampsBelowMin()
    {
        var svc = new QuestionService();
        var t = svc.GetTier(-1);
        Assert.NotNull(t);
        Assert.IsType<AddSubtractTier>(t);
    }
}
