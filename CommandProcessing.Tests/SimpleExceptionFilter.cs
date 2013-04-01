namespace CommandProcessing.Tests
{
    using System;
    using System.Diagnostics;
    using CommandProcessing.Filters;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class SimpleExceptionFilter : ExceptionFilterAttribute
    {
        private readonly bool handle;

        private readonly string name;

        public SimpleExceptionFilter(string name)
            : this(name, false)
        {
        }

        public SimpleExceptionFilter(string name, bool handle)
        {
            this.name = name;
            this.handle = handle;
        }

        public override void OnException(HandlerExecutedContext handlerExecutedContext)
        {
            base.OnException(handlerExecutedContext);
            if (this.handle)
            {
                handlerExecutedContext.Result = true;
                Trace.WriteLine("Exception handled by  " + this.name);
            }
        }
    }
}