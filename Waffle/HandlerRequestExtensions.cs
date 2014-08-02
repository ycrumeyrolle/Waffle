namespace Waffle
{
    using System;
    using Waffle.Commands;

    /// <summary>
    /// Contains extension methods for the handler request.
    /// </summary>
    public static class HandlerRequestExtensions
    {    
        /// <summary>
        /// Creates an <see cref="HandlerResponse"/>.
        /// </summary>
        /// <typeparam name="TResult">The value result type.</typeparam>
        /// <param name="request">The command request.</param>
        /// <returns>An <see cref="HandlerResponse"/> </returns>
        public static HandlerResponse CreateResponse(this CommandHandlerRequest request)
        {
            return new HandlerResponse(request);
        }

#if LOOSE_CQRS
        /// <summary>
        /// Creates an <see cref="HandlerResponse"/>.
        /// </summary>
        /// <typeparam name="TResult">The value result type.</typeparam>
        /// <param name="request">The command request.</param>
        /// <param name="value">The response value.</param>
        /// <returns>An <see cref="HandlerResponse"/> </returns>
        public static HandlerResponse CreateResponse<TResult>(this CommandHandlerRequest request, TResult value)
        {
            return new HandlerResponse(request, value);
        }
#endif

        /// <summary>
        /// Create an error <see cref="HandlerResponse"/>.
        /// </summary>
        /// <param name="request">The command request.</param>
        /// <param name="exception">The error.</param>
        /// <returns>An <see cref="HandlerResponse"/> </returns>
        public static HandlerResponse CreateErrorResponse(this CommandHandlerRequest request, Exception exception)
        {
            return new HandlerResponse(request, exception);
        }
    }
}
