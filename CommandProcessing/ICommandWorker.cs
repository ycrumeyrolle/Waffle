namespace CommandProcessing
{
    public interface ICommandWorker
    {
        TResult Execute<TCommand, TResult>(ICommandProcessor processor, HandlerRequest request) where TCommand : ICommand;
    }
}