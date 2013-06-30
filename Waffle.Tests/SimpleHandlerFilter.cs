namespace Waffle.Tests
{
    using System;
    using Waffle.Filters;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class SimpleHandlerFilter : HandlerFilterAttribute
    {
    }
}