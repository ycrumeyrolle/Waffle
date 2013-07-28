namespace Waffle.Tests.Integration.WaitList
{
    using Waffle.Commands;
    using Waffle.Filters;

    public class WaitList : CommandHandler<AddSeatsToWaitList>
    {
        private readonly ISpy spy;

        public WaitList(ISpy spy)
        {
            this.spy = spy;
        }

        /// <summary>
        /// Handle the command.
        /// </summary>
        /// <param name="command">The <see cref="ICommand"/> to process.</param>
        /// <param name="context">The <see cref="CommandHandlerContext"/>.</param>
        public override void Handle(AddSeatsToWaitList command, CommandHandlerContext context)
        {
            this.spy.Spy("AddSeatsToWaitList");
        }
    }
}
