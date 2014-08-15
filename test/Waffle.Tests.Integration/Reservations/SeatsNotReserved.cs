namespace Waffle.Tests.Integration.Reservations
{
    using System;
    using Waffle.Events;

    public class SeatsNotReserved : IEvent
    {
        public SeatsNotReserved(Guid sourceId)
        {
            this.SourceId = sourceId;
        }

        public Guid SourceId { get; private set; }
    }
}
