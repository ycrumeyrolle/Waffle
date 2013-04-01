namespace CommandProcessing.MiniProfiler
{
    using System;
    using System.Collections.Generic;
    using CommandProcessing.Filters;
    using StackExchange.Profiling;

    public class ProfileFilter : HandlerFilterAttribute
    {
        private const string Key = "__ProfileFilterKey";

        public override void OnCommandExecuting(HandlerContext context)
        {
            var stack = GetStack(context); 
            if (stack == null)
            {
                stack = new Stack<IDisposable>();
                context.Items[Key] = stack;
            }

            var profiler = MiniProfiler.Current;
            if (profiler != null)
            {
                var step = profiler.Step(context.Descriptor.Name);
                stack.Push(step);
            }
        }

        public override void OnCommandExecuted(HandlerExecutedContext context)
        {
            var stack = GetStack(context.HandlerContext);
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
