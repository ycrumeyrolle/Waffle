namespace CommandProcessing
{
    /// <summary>
    /// Represents a worker for command. 
    /// </summary>
    public interface ICommandWorker
    {
        /// <summary>
        /// Execute the request via the worker. 
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        TResult Execute<TCommand, TResult>(HandlerRequest request) where TCommand : ICommand;
    }
}