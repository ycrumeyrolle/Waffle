namespace Waffle.Queuing
{
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;

    /// <summary>
    /// Represents a strategy that enqueue the requests.
    /// </summary>
    public sealed class QueueStrategy : IQueueStrategy
    {
        /// <inheritdocs />
        public async Task<HandlerResponse> ExecuteAsync(CommandHandlerRequest request, CancellationToken cancellationToken)
        {
            ICommandSender sender = request.Configuration.Services.GetCommandSender();
            await sender.SendAsync(request.Command, cancellationToken);
            return new HandlerResponse(request);
        }
    }
}
