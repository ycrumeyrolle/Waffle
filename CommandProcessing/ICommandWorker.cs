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
        /// <typeparam name="TCommand">The type of the command.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="request">The <see cref="HandlerRequest"/> to execute.</param>
        /// <returns>The result of the command, if any.</returns>
        TResult Execute<TCommand, TResult>(HandlerRequest request) where TCommand : ICommand;
    }
}