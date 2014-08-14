namespace Waffle.Sample.WaitList
{
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Filters;
    using Waffle.Sample.Messaging.Orders;

    public class WaitList : MessageHandler, IAsyncCommandHandler<AddSeatsToWaitList>
    {
        public WaitList()
        {
        }

        /// <summary>
        /// Handle the command.
        /// </summary>
        /// <param name="command">The <see cref="ICommand"/> to process.</param>
         public Task HandleAsync(AddSeatsToWaitList command)
        {
            return Task.FromResult(0);
        }
    }
}
