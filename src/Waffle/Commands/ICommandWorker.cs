namespace Waffle.Commands
{
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a worker for command. 
    /// </summary>
    public interface ICommandWorker
    {
        /// <summary>
        /// Execute the request via the worker. 
        /// </summary>
        /// <param name="request">The <see cref="CommandHandlerRequest"/> to execute.</param>
        /// <returns>The result of the command, if any.</returns>
        Task<HandlerResponse> ExecuteAsync(CommandHandlerRequest request);
    }
}