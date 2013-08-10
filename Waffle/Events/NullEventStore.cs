namespace Waffle.Events
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Tasks;

    /// <summary>
    /// Null implementation of the <see cref="IEventStore"/>.
    /// 
    /// </summary>
    public class NullEventStore : IEventStore
    {
        private static readonly IEvent[] EmptyEvents = new IEvent[0];

        /// <summary>
        /// Stores an event.
        /// </summary>
        /// <param name="event">The <see cref="IEvent"/> to store.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="Task"/> of the storing.</returns>
        public Task StoreAsync(IEvent @event, CancellationToken cancellationToken)
        {
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
            return TaskHelpers.FromResult<ICollection<IEvent>>(EmptyEvents);
        }
    }
}