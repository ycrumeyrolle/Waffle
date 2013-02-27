namespace CommandProcessing.Filters
{
    public class HandlerExecutingContext
    {
        public HandlerExecutingContext(HandlerContext context)
        {
            this.CommandContext = context;
        }

        public HandlerResult Result { get; set; }

        public HandlerContext CommandContext { get; set; }
    }
}