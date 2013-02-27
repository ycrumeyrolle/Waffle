namespace CommandProcessing.Filters
{
    public interface IHandlerFilter : IFilter
    {
        void OnCommandExecuting(HandlerExecutingContext context);

        void OnCommandExecuted(HandlerExecutedContext context);
    }
}