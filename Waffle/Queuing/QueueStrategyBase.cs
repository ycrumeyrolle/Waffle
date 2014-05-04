namespace Waffle.Queuing
{
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;

    public abstract class QueueStrategyBase
    {
        public abstract Task<HandlerResponse> ExecuteAsync(CommandHandlerRequest request, CancellationToken cancellationToken);
    }
}
