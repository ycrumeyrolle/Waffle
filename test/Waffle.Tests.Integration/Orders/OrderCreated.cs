namespace Waffle.Tests.Integration.Orders
{
    using System;
    using Waffle.Events;

    public class OrderCreated : IEvent
    {
        public OrderCreated(Guid sourceId, int count)
        {
            this.SourceId = sourceId;
            this.Count = count;
        }

        /// <summary>
        /// Gets the identifier of the source originating the event.
        /// </summary>
        public Guid SourceId { get; private set; }

        public int Count { get; private set; }
    }
}
