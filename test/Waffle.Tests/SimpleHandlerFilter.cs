namespace Waffle.Tests
{
    using System;
    using Waffle.Commands;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class SimpleCommandHandlerFilter : CommandHandlerFilterAttribute
    {
    }
}