namespace Waffle.Queuing
{
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;
  
    /// <summary>
    /// Represents a queue strategy that determines the way to enqueue requests.
    /// </summary>
    public interface IQueueStrategy
    {
        /// <summary>
        /// Executes the queuing strategy for the request.
        /// </summary>
        /// <param name="request">The request to enqueue.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>Returns a task that represent the completion of the strategy.</returns>
        Task<HandlerResponse> ExecuteAsync(CommandHandlerRequest request, CancellationToken cancellationToken);
    }
}
