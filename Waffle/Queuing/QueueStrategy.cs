namespace Waffle.Queuing
{
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;

    public sealed class QueueStrategy : QueueStrategyBase
    {
        public override async Task<HandlerResponse> ExecuteAsync(CommandHandlerRequest request, CancellationToken cancellationToken)
        {
            ICommandSender sender = request.Configuration.Services.GetCommandSender();
            await sender.SendAsync(request.Command, cancellationToken);
            return new HandlerResponse(request);
        }
    }
}
