namespace Waffle.Sample.Payments
{
    using System;
    using Waffle.Events;

    public class PaymentAccepted : IEvent
    {
        public PaymentAccepted(Guid sourceId)
        {
            this.SourceId = sourceId;
        }

        /// <summary>
        /// Gets the identifier of the source originating the event.
        /// </summary>
        public Guid SourceId { get; private set; }
    }
}
