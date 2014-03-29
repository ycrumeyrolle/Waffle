namespace Waffle.Tests.Integration.Orders
{
    using System;
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Events;
    using Waffle.Filters;

    public class Order : MessageHandler,
        IAsyncCommandHandler<PlaceOrder>,
        IAsyncEventHandler<OrderConfirmed>
    {
        private readonly ISpy spy;

        public Order()
            : this(new NullSpy())
        {
        }

        public Order(ISpy spy)
        {
            this.spy = spy;
            this.Id = Guid.NewGuid();
            this.spy.Spy("Order ctor");
        }

        public Guid Id { get; set; }
        private class ExceptionHandlerAttribute : ExceptionFilterAttribute
        {
            public override void OnException(CommandHandlerExecutedContext handlerExecutedContext)
            {
                base.OnException(handlerExecutedContext);

            }
        }

        [ExceptionHandler]
        public Task HandleAsync(PlaceOrder command)
        {            
            this.spy.Spy("PlaceOrder");

            if (command.Count > 100)
            {
                throw new ArgumentOutOfRangeException("command", "Too much orders !");
            }

            OrderCreated orderCreated = new OrderCreated(this.Id, command.Count);
            return this.CommandContext.Request.Processor.PublishAsync(orderCreated);
        }

        public Task HandleAsync(OrderConfirmed @event)
        {
            this.spy.Spy("OrderConfirmed");
            return Task.FromResult(0);
        }
    }
}
