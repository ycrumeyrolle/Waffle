namespace CommandProcessing.MiniProfiler
{
    using System;
    using System.Collections.Generic;
    using CommandProcessing.Filters;
    using StackExchange.Profiling;

    public class ProfileFilter : FilterAttribute, IHandlerFilter
    {
        private const string Key = "__ProfileFilterKey";
        
        public void OnCommandExecuting(HandlerExecutingContext context)
        {
            var stack = GetStack(context.CommandContext); 
            if (stack == null)
            {
                stack = new Stack<IDisposable>();
                context.CommandContext.Items[Key] = stack;
            }

            var profiler = MiniProfiler.Current;
            if (profiler != null)
            {
                var step = profiler.Step(context.CommandContext.Descriptor.Name);
                stack.Push(step);
            }
        }

        public void OnCommandExecuted(HandlerExecutedContext context)
        {
            var stack = GetStack(context.Context.CommandContext);
            if (stack != null && stack.Count > 0)
            {
                var disposable = stack.Pop();
                disposable.Dispose();
            }
        }

        private static Stack<IDisposable> GetStack(HandlerContext context)
        {
            var stack = context.Items[Key] as Stack<IDisposable>;
            return stack;
        }
    }
}
