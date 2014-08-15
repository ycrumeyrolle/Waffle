namespace Waffle.Tests
{
    using System.ComponentModel.DataAnnotations;
    using Waffle.Commands;

    public class SimpleCommand : ICommand
    {
        public int Property1 { get; set; }

        [Required]
        public string Property2 { get; set; }

        public string Property3 { get; set; }
    }
}