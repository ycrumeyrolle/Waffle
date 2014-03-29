namespace Waffle.Sample.Orders
{
    using System;
    using System.Threading.Tasks;
    using Waffle.Events;
    using Waffle.Sample.Messaging.Orders;
    using Waffle.Sample.Payments;
    using Waffle.Sample.Reservations;
    using Waffle.Sample.WaitList;

    public class OrderProcessManager : MessageHandler,
        IAsyncEventHandler<OrderCreated>,
        IAsyncEventHandler<SeatsReserved>,
        IAsyncEventHandler<SeatsNotReserved>,
        IAsyncEventHandler<PaymentAccepted>
    {
        public OrderProcessManager()
        {
            this.Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }

        /// <summary>
        /// Handle the event.
        /// </summary>
        /// <param name="event">The <see cref="IEvent"/> to handle.</param>
        public Task HandleAsync(OrderCreated @event)
        {
            MakeReservation makeReservation = new MakeReservation();
            return this.EventContext.Request.Processor.ProcessAsync(makeReservation); 
        }

        /// <summary>
        /// Handle the event.
        /// </summary>
        /// <param name="event">The <see cref="IEvent"/> to handle.</param>
        public Task HandleAsync(SeatsReserved @event)
        {
            MakePayment makePayment = new MakePayment();
            return this.EventContext.Request.Processor.ProcessAsync(makePayment); 
        }

        /// <summary>
        /// Handle the event.
        /// </summary>
        /// <param name="event">The <see cref="IEvent"/> to handle.</param>
        public Task HandleAsync(SeatsNotReserved @event)
        {
            AddSeatsToWaitList addSeatsToWaitList = new AddSeatsToWaitList();
            return this.EventContext.Request.Processor.ProcessAsync(addSeatsToWaitList);
        }

        /// <summary>
        /// Handle the event.
        /// </summary>
        /// <param name="event">The <see cref="IEvent"/> to handle.</param>
        public Task HandleAsync(PaymentAccepted @event)
        {
            OrderConfirmed orderConfirmed = new OrderConfirmed(this.Id);
            return this.EventContext.Request.Processor.PublishAsync(orderConfirmed);
        }
    }
}
