namespace Waffle.Queuing
{
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;
    
    public sealed class NoQueueStrategy : QueueStrategyBase
    {
        public override Task<HandlerResponse> ExecuteAsync(CommandHandlerRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult<HandlerResponse>(null);
        }
    }
}
