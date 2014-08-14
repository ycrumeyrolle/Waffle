namespace Waffle.Sample.Orders
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Waffle.Commands;

    public class PlaceOrder : ICommand
    {
        public PlaceOrder()
        {
            this.Seats = new List<int>();
        }

        public ICollection<int> Seats { get; private set; }

        [Required]
        public int ConferenceId { get; set; }
    }
}
