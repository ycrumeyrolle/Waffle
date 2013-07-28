namespace Waffle.Tests.Integration.Orders
{
    using System;
    using Waffle.Events;
    using Waffle.Tests.Integration.Payments;
    using Waffle.Tests.Integration.Reservations;
    using Waffle.Tests.Integration.WaitList;

    [HandlerLifetime(Filters.HandlerLifetime.PerRequest)]
    public class OrderProcessManager : MessageHandler,
        IEventHandler<OrderCreated>,
        IEventHandler<SeatsReserved>,
        IEventHandler<SeatsNotReserved>,
        IEventHandler<PaymentAccepted>
    {
        private readonly ISpy spy;

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
        /// <param name="context">The <see cref="EventHandlerContext"/>.</param>
        public void Handle(OrderCreated @event, EventHandlerContext context)
        {
            this.spy.Spy("OrderCreated");
            MakeReservation makeReservation = new MakeReservation();
            context.Request.Processor.Process(makeReservation); 
        }

        /// <summary>
        /// Handle the event.
        /// </summary>
        /// <param name="event">The <see cref="IEvent"/> to handle.</param>
        /// <param name="context">The <see cref="EventHandlerContext"/>.</param>
        public void Handle(SeatsReserved @event, EventHandlerContext context)
        {
            this.spy.Spy("SeatsReserved");
            MakePayment makePayment = new MakePayment();
            context.Request.Processor.Process(makePayment); 
        }

        /// <summary>
        /// Handle the event.
        /// </summary>
        /// <param name="event">The <see cref="IEvent"/> to handle.</param>
        /// <param name="context">The <see cref="EventHandlerContext"/>.</param>
        public void Handle(SeatsNotReserved @event, EventHandlerContext context)
        {
            this.spy.Spy("SeatsNotReserved");
            AddSeatsToWaitList addSeatsToWaitList = new AddSeatsToWaitList();
            context.Request.Processor.Process(addSeatsToWaitList);
        }

        /// <summary>
        /// Handle the event.
        /// </summary>
        /// <param name="event">The <see cref="IEvent"/> to handle.</param>
        /// <param name="context">The <see cref="EventHandlerContext"/>.</param>
        public void Handle(PaymentAccepted @event, EventHandlerContext context)
        {
            this.spy.Spy("PaymentAccepted");
            OrderConfirmed orderConfirmed = new OrderConfirmed(this.Id);
            context.Request.Processor.Publish(orderConfirmed);
        }
    }
}
