namespace Waffle.Events
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IEventHandlerInvoker
    {
        Task InvokeHandlerAsync(EventHandlerContext context, CancellationToken cancellationToken);
    }
}