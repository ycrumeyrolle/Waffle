namespace Waffle.Events
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents the methods to store events.
    /// </summary>
    public interface IEventStore
    {
        /// <summary>
        /// Stores an event.
        /// </summary>
        /// <param name="event">The <see cref="IEvent"/> to store.</param>
        /// <param name="eventName">The event name.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="Task"/> of the storing.</returns>
        Task StoreAsync(IEvent @event, string eventName, CancellationToken cancellationToken);

        /// <summary>
        /// Load a collection of events.
        /// </summary>
        /// <param name="sourceId">The event source identifier.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="Task"/> of <see cref="ICollection{IEvent}"/> containing the <see cref="IEvent"/>.</returns>
        Task<ICollection<IEvent>> LoadAsync(Guid sourceId, CancellationToken cancellationToken);
    }
}