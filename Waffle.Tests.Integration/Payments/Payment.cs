namespace Waffle.Tests.Integration.Payments
{
    using System;
    using Waffle.Commands;
    using Waffle.Filters;

    public class Payment : CommandHandler<MakePayment>
    {
        private readonly ISpy spy;

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
        public override void Handle(MakePayment command, CommandHandlerContext context)
        {
            this.spy.Spy("MakePayment");
            PaymentAccepted paymentAccepted = new PaymentAccepted(this.Id);
            context.Request.Processor.Publish(paymentAccepted);
        }
    }
}
