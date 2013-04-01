namespace CommandProcessing.Tests
{
    using System;
    using CommandProcessing.Filters;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class SimpleHandlerFilter : HandlerFilterAttribute
    {
    }
}