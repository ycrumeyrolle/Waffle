namespace Waffle.Tests.Integration.Orders
{
    using System;
    using System.Threading.Tasks;
    using Waffle;
    using Waffle.Events;
    using Waffle.Tests.Integration.Payments;
    using Waffle.Tests.Integration.Reservations;
    using Waffle.Tests.Integration.WaitList;

    public class OrderProcessManager : MessageHandler,
        IAsyncEventHandler<OrderCreated>,
        IAsyncEventHandler<SeatsReserved>,
        IAsyncEventHandler<SeatsNotReserved>,
        IAsyncEventHandler<PaymentAccepted>
    {
        private readonly ISpy spy;

        public OrderProcessManager()
            : this(new NullSpy())
        {
        }

        public OrderProcessManager(ISpy spy)
        {
            this.spy = spy;
            this.Id = Guid.NewGuid();
            this.spy.Spy("OrderProcessManager ctor");
        }

        public Guid Id { get; private set; }

        /// <summary>
        /// Handle the event.
        /// </summary>
        /// <param name="event">The <see cref="IEvent"/> to handle.</param>
        public Task HandleAsync(OrderCreated @event)
        {
            this.spy.Spy("OrderCreated");
            if (@event.Count == -1)
            {
                throw new InvalidOperationException("Exception on Event");
            }

            MakeReservation makeReservation = new MakeReservation();
            return this.EventContext.Request.Processor.ProcessAsync(makeReservation); 
        }

        /// <summary>
        /// Handle the event.
        /// </summary>
        /// <param name="event">The <see cref="IEvent"/> to handle.</param>
        public Task HandleAsync(SeatsReserved @event)
        {
            this.spy.Spy("SeatsReserved");
            MakePayment makePayment = new MakePayment();
            return this.EventContext.Request.Processor.ProcessAsync(makePayment); 
        }

        /// <summary>
        /// Handle the event.
        /// </summary>
        /// <param name="event">The <see cref="IEvent"/> to handle.</param>
        public Task HandleAsync(SeatsNotReserved @event)
        {
            this.spy.Spy("SeatsNotReserved");
            AddSeatsToWaitList addSeatsToWaitList = new AddSeatsToWaitList();
            return this.EventContext.Request.Processor.ProcessAsync(addSeatsToWaitList);
        }

        /// <summary>
        /// Handle the event.
        /// </summary>
        /// <param name="event">The <see cref="IEvent"/> to handle.</param>
        public Task HandleAsync(PaymentAccepted @event)
        {
            this.spy.Spy("PaymentAccepted");
            OrderConfirmed orderConfirmed = new OrderConfirmed(this.Id);
            return this.EventContext.Request.Processor.PublishAsync(orderConfirmed);
        }
    }
}
