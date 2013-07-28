namespace Waffle.Tests.Integration.Reservations
{
    using System;
    using Waffle.Commands;
    using Waffle.Events;
    using Waffle.Filters;
    using Waffle.Tests.Integration.Orders;

    public class Reservation : MessageHandler,
        ICommandHandler<MakeReservation>,
        IEventHandler<OrderConfirmed>
    {
        private readonly ISpy spy;

        public Reservation(ISpy spy)
        {
            this.spy = spy;
            this.Id = Guid.NewGuid();
            this.spy.Spy("Reservation ctor");
        }

        public Guid Id { get; set; }

        public void Handle(MakeReservation command, CommandHandlerContext context)
        {
            this.spy.Spy("MakeReservation");

            SeatsReserved seatsReserved = new SeatsReserved(this.Id);
            context.Request.Processor.Publish(seatsReserved);
        }

        public void Handle(OrderConfirmed @event, EventHandlerContext context)
        {
            this.spy.Spy("OrderConfirmed");
        }
    }
}
