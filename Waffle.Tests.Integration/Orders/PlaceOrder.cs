namespace Waffle.Tests.Integration.Orders
{
    using Waffle.Commands;

    public class PlaceOrder : ICommand
    {
        public PlaceOrder(int count)
        {
            this.Count = count;
        }

        public int Count { get; private set; }
    }
}
