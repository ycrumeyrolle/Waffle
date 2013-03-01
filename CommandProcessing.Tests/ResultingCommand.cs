namespace CommandProcessing.Tests
{
    using System.ComponentModel.DataAnnotations;
    using CommandProcessing;

    public class ResultingCommand : Command
    {
        public int Property1 { get; set; }

        [Required]
        public string Property2 { get; set; }
    }
}