namespace Waffle
{
    using System;
    using Waffle.Commands;

    public static class HandlerRequestExtensions
    {
        public static HandlerResponse CreateResponse<TResult>(this CommandHandlerRequest request, TResult value)
        {
            return new HandlerResponse(request, value);
        }

        public static HandlerResponse CreateErrorResponse(this CommandHandlerRequest request, Exception exception)
        {
            return new HandlerResponse(request, exception);
        }
    }
}
