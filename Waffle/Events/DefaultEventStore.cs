namespace Waffle.Events
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Tasks;

    /// <summary>
    /// Default implementation of the <see cref="IEventStore"/>.
    /// </summary>
    public class DefaultEventStore : IEventStore
    {
        private readonly ConcurrentQueue<IEvent> queue = new ConcurrentQueue<IEvent>();

        /// <summary>
        /// Stores an event.
        /// </summary>
        /// <param name="event">The <see cref="IEvent"/> to store.</param>
        /// <param name="eventName">The event name.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="Task"/> of the storing.</returns>
        public Task StoreAsync(IEvent @event, string eventName, CancellationToken cancellationToken)
        {
            this.queue.Enqueue(@event);
            return TaskHelpers.Completed();
        }

        /// <summary>
        /// Load a collection of events.
        /// </summary>
        /// <param name="sourceId">The event source identifier.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="Task"/> of <see cref="ICollection{IEvent}"/> containing the <see cref="IEvent"/>.</returns>
        public Task<ICollection<IEvent>> LoadAsync(Guid sourceId, CancellationToken cancellationToken)
        {
            IEnumerable<IEvent> value = this.queue.Where(e => e.SourceId == sourceId);
            return TaskHelpers.FromResult<ICollection<IEvent>>(new ReadOnlyCollection<IEvent>(value.ToList()));
        }
    }
}