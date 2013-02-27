namespace CommandProcessing.Tests
{
    using System;
    using System.Diagnostics;
    using CommandProcessing.Filters;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class SimpleExceptionFilter : FilterAttribute, IExceptionFilter
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

        public void OnException(ExceptionContext exceptionContext)
        {
            exceptionContext.ExceptionHandled = exceptionContext.ExceptionHandled || this.handle;
            Trace.WriteLine("Exception handled by  " + this.name + " : " + this.handle);
        }
    }
}