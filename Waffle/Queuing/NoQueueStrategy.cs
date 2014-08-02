namespace Waffle.Queuing
{
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;
   
    /// <summary>
    /// Represents a strategy that do not enqueue the requests.
    /// </summary>
    public sealed class NoQueueStrategy : IQueueStrategy
    {
        /// <inheritdocs />
        public Task<HandlerResponse> ExecuteAsync(CommandHandlerRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult<HandlerResponse>(null);
        }
    }
}
