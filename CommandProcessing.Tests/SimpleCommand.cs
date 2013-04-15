namespace CommandProcessing.Tests
{
    using System.ComponentModel.DataAnnotations;
    using CommandProcessing;

    public class SimpleCommand : Command
    {
        public int Property1 { get; set; }

        [Required]
        public string Property2 { get; set; }

        public string Property3 { get; set; }
    }
}