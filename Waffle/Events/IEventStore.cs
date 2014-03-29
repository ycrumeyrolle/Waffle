namespace Waffle.Events
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
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
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="Task"/> of the storing.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "event", Justification = "By design.")]
        Task StoreAsync(IEvent @event, CancellationToken cancellationToken);

        /// <summary>
        /// Load a collection of events.
        /// </summary>
        /// <param name="sourceId">The event source identifier.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="Task"/> of <see cref="ICollection{IEvent}"/> containing the <see cref="IEvent"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nesting is required to return Task.")]
        Task<ICollection<IEvent>> LoadAsync(Guid sourceId, CancellationToken cancellationToken);
    }
}