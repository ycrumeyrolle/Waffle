namespace Waffle.Sample.Reservations
{
    using System;
    using Waffle.Events;

    public class SeatsReserved : IEvent
    {
        public SeatsReserved(Guid sourceId)
        {
            this.SourceId = sourceId;
        }

        public Guid SourceId { get; private set; }
    }
}
