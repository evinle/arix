namespace ArixBack.Services.Questions
{
    public record Question(string Id, string Text, string Answer);

    public interface IQuestionTier
    {
        Question Generate();
        bool Validate(Question q, string answer);
    }
}
