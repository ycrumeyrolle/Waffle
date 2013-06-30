namespace Waffle.MiniProfiler
{
    using System;
    using System.Collections.Generic;
    using StackExchange.Profiling;
    using Waffle.Filters;

    /// <summary>
    /// Represents a filter to profile handlers execution.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class ProfileFilterAttribute : HandlerFilterAttribute
    {
        private const string Key = "__ProfileFilterKey";

        /// <summary>
        /// Occurs before the handle method is invoked.
        /// </summary>
        /// <param name="handlerContext">The handler context.</param>
        public override void OnCommandExecuting(HandlerContext handlerContext)
        {
            if (handlerContext == null)
            {
                throw new ArgumentNullException("handlerContext");
            }

            Stack<IDisposable> stack = GetStack(handlerContext); 
            if (stack == null)
            {
                stack = new Stack<IDisposable>();
                handlerContext.Items[Key] = stack;
            }

            MiniProfiler profiler = MiniProfiler.Current;
            if (profiler != null)
            {
                IDisposable step = profiler.Step(handlerContext.Descriptor.Name);
                stack.Push(step);
            }
        }

        /// <summary>
        /// Occurs after the handle method is invoked.
        /// </summary>
        /// <param name="handlerExecutedContext">The handler executed context.</param>
        public override void OnCommandExecuted(HandlerExecutedContext handlerExecutedContext)
        {
            if (handlerExecutedContext == null)
            {
                throw new ArgumentNullException("handlerExecutedContext");
            }

            Stack<IDisposable> stack = GetStack(handlerExecutedContext.HandlerContext);
            if (stack != null && stack.Count > 0)
            {
                IDisposable disposable = stack.Pop();
                disposable.Dispose();
            }
        }

        private static Stack<IDisposable> GetStack(HandlerContext context)
        {
            Stack<IDisposable> stack = context.Items[Key] as Stack<IDisposable>;
            return stack;
        }
    }
}
