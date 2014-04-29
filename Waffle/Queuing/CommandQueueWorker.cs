namespace Waffle.Queuing
{
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;

    /// <summary>
    /// Implementation of the <see cref="ICommandWorker"/> 
    /// </summary>
    public class CommandQueueWorker : ICommandWorker
    {
        public async Task<HandlerResponse> ExecuteAsync(CommandHandlerRequest request)
        {
            ICommandSender sender = request.Configuration.Services.GetCommandSender();
            await sender.SendAsync(request.Command, default(CancellationToken));
            return new HandlerResponse(request);
        }
    }
}
