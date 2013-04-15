namespace CommandProcessing.Caching
{
    using System;

    /// <summary>
    /// Represents a filter to bypass cache command result.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class NoCacheAttribute : CacheAttribute
    {
        public override void OnCommandExecuting(Filters.HandlerContext handlerContext)
        {
            // Bypass base cache processing
        }

        public override void OnCommandExecuted(Filters.HandlerExecutedContext handlerExecutedContext)
        {
            // Bypass base cache processing
        }
    }
}
