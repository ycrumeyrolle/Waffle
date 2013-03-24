namespace CommandProcessing
{
    using CommandProcessing.Filters;

    public abstract class Handler
    {
        public CommandProcessor Processor { get; internal set; }

        public HandlerContext Context { get; internal set; }

        public abstract object Handle(ICommand command);
    }
}