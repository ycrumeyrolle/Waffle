namespace Waffle
{
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Events;

    /// <summary>
    /// Represents a worker for events. 
    /// </summary>
    public interface IEventWorker
    {
        /// <summary>
        /// Execute the request via the worker. 
        /// </summary>
        /// <param name="request">The <see cref="EventHandlerRequest"/> to execute.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task"/> of the event.</returns>
        Task PublishAsync(EventHandlerRequest request, CancellationToken cancellationToken);
    }
}