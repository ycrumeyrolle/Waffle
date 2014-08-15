namespace Waffle.Tests.Interception
{
    public class BadService
    {
        public BadService(string value)
        {
            this.Value = value;
        }
        
        public string Value { get; set; }
    }
}
