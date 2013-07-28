namespace Waffle.Tests.Integration.Orders
{
    using System;
    using Waffle.Commands;
    using Waffle.Events;
    using Waffle.Filters;

    public class Order : MessageHandler,
        ICommandHandler<PlaceOrder>,
        IEventHandler<OrderConfirmed>
    {
        private readonly ISpy spy;

        public Order(ISpy spy)
        {
            this.spy = spy;
            this.Id = Guid.NewGuid();
            this.spy.Spy("Order ctor");
        }

        public Guid Id { get; set; }

        public void Handle(PlaceOrder command, CommandHandlerContext context)
        {
            this.spy.Spy("PlaceOrder");
            OrderCreated orderCreated = new OrderCreated(this.Id);
            context.Request.Processor.Publish(orderCreated);
        }

        public void Handle(OrderConfirmed @event, EventHandlerContext context)
        {
            this.spy.Spy("OrderConfirmed");
        }
    }
}
