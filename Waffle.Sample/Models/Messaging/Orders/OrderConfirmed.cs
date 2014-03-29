namespace Waffle.Sample.Orders
{
    using System;
    using Waffle.Events;

    public class OrderConfirmed : IEvent
    {
        public OrderConfirmed(Guid sourceId)
        {
            this.SourceId = sourceId;
        }

        /// <summary>
        /// Gets the identifier of the source originating the event.
        /// </summary>
        public Guid SourceId { get; private set; }
    }
}
