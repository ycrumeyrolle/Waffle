namespace Waffle
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Events;
    using Waffle.Internal;

    /// <summary>
    /// Extensions methods for <see cref="IEventStore"/>.
    /// </summary>
    public static class EventStoreExtensions
    {
        /// <summary>
        /// Stores an event.
        /// </summary>
        /// <param name="eventStore">The <see cref="IEventStore"/>.</param>
        /// <param name="event">The <see cref="IEvent"/> to store.</param>
        /// <param name="eventName">The event name.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public static void Store(this IEventStore eventStore, IEvent @event, string eventName, CancellationToken cancellationToken)
        {
            if (eventStore == null)
            {
                throw Error.ArgumentNull("eventStore");
            }

            Task task = eventStore.StoreAsync(@event, eventName, cancellationToken);
            task.Wait(cancellationToken);
        }

        /// <summary>
        /// Load a collection of events.
        /// </summary>
        /// <param name="eventStore">The <see cref="IEventStore"/>.</param>
        /// <param name="sourceId">The event source identifier.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="ICollection{IEvent}"/> containing the <see cref="IEvent"/>.</returns>
        public static ICollection<IEvent> Load(this IEventStore eventStore, Guid sourceId, CancellationToken cancellationToken)
        {
            if (eventStore == null)
            {
                throw Error.ArgumentNull("eventStore");
            }

            Task<ICollection<IEvent>> task = eventStore.LoadAsync(sourceId, cancellationToken);
            return task.Result;
        }
    }
}
