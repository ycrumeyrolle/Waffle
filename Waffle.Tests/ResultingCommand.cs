namespace Waffle.Tests
{
    using System.ComponentModel.DataAnnotations;
    using Waffle;

    public class ResultingCommand : Command
    {
        public int Property1 { get; set; }

        [Required]
        public string Property2 { get; set; }
    }
}