namespace CommandProcessing
{
    public interface IHandler<in TCommand, out TResult> where TCommand : ICommand
    {
        TResult Handle(TCommand command);
    }
}
