namespace Waffle.Tests.Integration.Orders
{
    using System;
    using Waffle.Events;

    public class OrderCreated : IEvent
    {
        public OrderCreated(Guid sourceId)
        {
            this.SourceId = sourceId;
        }

        /// <summary>
        /// Gets the identifier of the source originating the event.
        /// </summary>
        public Guid SourceId { get; private set; }
    }
}
