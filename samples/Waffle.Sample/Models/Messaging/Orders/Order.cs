namespace Waffle.Sample.Orders
{
    using System;
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Events;

    public class Order : MessageHandler,
        IAsyncCommandHandler<PlaceOrder>,
        IAsyncEventHandler<OrderConfirmed>
    {
        public Order()
        {
            this.Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public Task HandleAsync(PlaceOrder command)
        {
            OrderCreated orderCreated = new OrderCreated(this.Id);
            return this.CommandContext.Request.Processor.PublishAsync(orderCreated);
        }

        public Task HandleAsync(OrderConfirmed @event)
        {
            return Task.FromResult(0);
        }
    }
}
