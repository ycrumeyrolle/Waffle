namespace Waffle.Sample.Payments
{
    using System;
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Filters;
    using Waffle.Sample.Messaging.Orders;

    public class Payment : CommandHandler, IAsyncCommandHandler<MakePayment>
    {
        public Payment()
        {
            this.Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        /// <summary>
        /// Handle the command.
        /// </summary>
        /// <param name="command">The <see cref="ICommand"/> to process.</param>
        public Task HandleAsync(MakePayment command)
        {
            PaymentAccepted paymentAccepted = new PaymentAccepted(this.Id);
            return this.CommandContext.Request.Processor.PublishAsync(paymentAccepted);
        }
    }
}
