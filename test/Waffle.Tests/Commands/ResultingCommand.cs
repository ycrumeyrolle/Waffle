namespace Waffle.Tests.Commands
{
    using System.ComponentModel.DataAnnotations;
    using Waffle.Commands;

    public class ResultingCommand : ICommand
    {
        public int Property1 { get; set; }

        [Required]
        public string Property2 { get; set; }
    }
}