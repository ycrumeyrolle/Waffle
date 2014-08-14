namespace Waffle.Tests.Integration.Payments
{
    using System;
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Filters;

    public class Payment : CommandHandler, IAsyncCommandHandler<MakePayment>
    {
        private readonly ISpy spy;

        public Payment()
            : this(new NullSpy())
        {
        }

        public Payment(ISpy spy)
        {
            this.spy = spy;
            this.Id = Guid.NewGuid();
            this.spy.Spy("Payment ctor");
        }

        public Guid Id { get; set; }

        /// <summary>
        /// Handle the command.
        /// </summary>
        /// <param name="command">The <see cref="ICommand"/> to process.</param>
        /// <param name="context">The <see cref="CommandHandlerContext"/>.</param>
        public Task HandleAsync(MakePayment command)
        {
            this.spy.Spy("MakePayment");
            PaymentAccepted paymentAccepted = new PaymentAccepted(this.Id);
            return this.CommandContext.Request.Processor.PublishAsync(paymentAccepted);
        }
    }
}
