namespace CommandProcessing.Tests.Interception
{
    public class BadService
    {
        public string Value { get; set; }

        public BadService(string value)
        {
            this.Value = value;
        }
    }
}
