namespace CommandProcessing.Tests
{
    using System;
    using CommandProcessing.Filters;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class SimpleHandlerFilter : FilterAttribute, IHandlerFilter
    {
        public void OnCommandExecuting(HandlerExecutingContext context)
        {
            throw new NotImplementedException();
        }

        public void OnCommandExecuted(HandlerExecutedContext context)
        {
            throw new NotImplementedException();
        }
    }
}