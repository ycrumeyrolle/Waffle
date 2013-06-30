namespace Waffle.Queries.Data.Tests
{
    using System.ComponentModel.DataAnnotations;

    public class FakeEntity
    {
        [Key]
        public string Property1 { get; set; }

        public int Property2 { get; set; }
    }
}