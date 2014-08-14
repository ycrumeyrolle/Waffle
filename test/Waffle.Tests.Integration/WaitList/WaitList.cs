namespace Waffle.Tests.Integration.WaitList
{
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Filters;

    public class WaitList : CommandHandler, IAsyncCommandHandler<AddSeatsToWaitList>
    {
        private readonly ISpy spy;

        public WaitList()
            : this(new NullSpy())
        {
        }

        public WaitList(ISpy spy)
        {
            this.spy = spy;
        }

        /// <summary>
        /// Handle the command.
        /// </summary>
        /// <param name="command">The <see cref="ICommand"/> to process.</param>
        /// <param name="context">The <see cref="CommandHandlerContext"/>.</param>
        public Task HandleAsync(AddSeatsToWaitList command)
        {
            this.spy.Spy("AddSeatsToWaitList");
            return Task.FromResult(0);
        }
    }
}
