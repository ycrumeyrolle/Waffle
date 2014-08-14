namespace Waffle.Results
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>Defines a command that asynchronously creates an <see cref="HandlerResponse"/>.</summary>
    public interface ICommandHandlerResult
    {
        /// <summary>Executes a command handler asynchronously.</summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous execution operation. The result contains the <see cref="HandlerResponse"/>.</returns>
        Task<HandlerResponse> ExecuteAsync(CancellationToken cancellationToken);
    }
}