namespace Waffle.Events
{
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a worker for events. 
    /// </summary>
    public interface IEventWorker
    {
        /// <summary>
        /// Execute the request via the worker. 
        /// </summary>
        /// <param name="request">The <see cref="EventHandlerRequest"/> to execute.</param>
        /// <returns>The <see cref="Task"/> of the event.</returns>
        Task PublishAsync(EventHandlerRequest request);
    }
}