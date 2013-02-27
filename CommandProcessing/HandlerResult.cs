namespace CommandProcessing
{
    public class HandlerResult
    {
        public HandlerResult(object value)
        {
            this.Value = value;
        }

        public object Value { get; set; }
    }
}