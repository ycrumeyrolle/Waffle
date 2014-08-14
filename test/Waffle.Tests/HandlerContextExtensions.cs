#if LOOSE_CQRS
namespace Waffle.Tests
{
    using Waffle.Filters;

    public static class HandlerContextExtensions
    {
        public static void SetResponse(this CommandHandlerExecutedContext handlerExecutedContext, object result)
        {
            handlerExecutedContext.Response = new HandlerResponse(handlerExecutedContext.Request, result);
        }
        public static void SetResponse(this CommandHandlerContext commandHandlerContext, object result)
        {
            commandHandlerContext.Response = new HandlerResponse(commandHandlerContext.Request, result);
        }
    }
}
#endif