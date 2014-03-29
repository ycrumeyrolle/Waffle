namespace Waffle.Sample.Reservations
{
    using System;
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Events;
    using Waffle.Filters;
    using Waffle.Sample.Messaging.Orders;
    using Waffle.Sample.Orders;

    public class Reservation : MessageHandler,
        IAsyncCommandHandler<MakeReservation>,
        IAsyncEventHandler<OrderConfirmed>
    {
        public Reservation()
        {
            this.Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public Task HandleAsync(MakeReservation command)
        {
            SeatsReserved seatsReserved = new SeatsReserved(this.Id);
            return this.CommandContext.Request.Processor.PublishAsync(seatsReserved);
        }

        public Task HandleAsync(OrderConfirmed @event)
        {
            return Task.FromResult(0);
        }
    }
}
