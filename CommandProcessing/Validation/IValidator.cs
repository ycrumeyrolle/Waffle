namespace CommandProcessing.Validation
{
    public interface ICommandValidator
    {
        bool Validate(ICommand command);
    }
}
