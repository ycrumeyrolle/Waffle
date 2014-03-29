namespace Waffle.Tests.Integration.Reservations
{
    using System;
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Events;
    using Waffle.Filters;
    using Waffle.Tests.Integration.Orders;

    public class Reservation : MessageHandler,
        IAsyncCommandHandler<MakeReservation>,
        IAsyncEventHandler<OrderConfirmed>
    {
        private readonly ISpy spy;

        public Reservation()
            : this(new NullSpy())
        {
        }

        public Reservation(ISpy spy)
        {
            this.spy = spy;
            this.Id = Guid.NewGuid();
            this.spy.Spy("Reservation ctor");
        }

        public Guid Id { get; set; }

        public Task HandleAsync(MakeReservation command)
        {
            this.spy.Spy("MakeReservation");

            SeatsReserved seatsReserved = new SeatsReserved(this.Id);
            return this.CommandContext.Request.Processor.PublishAsync(seatsReserved);
        }

        public Task HandleAsync(OrderConfirmed @event)
        {
            this.spy.Spy("OrderConfirmed");
            return Task.FromResult(0);
        }
    }
}
