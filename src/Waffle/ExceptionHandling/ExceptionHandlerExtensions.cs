namespace Waffle.ExceptionHandling
{
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Internal;
    using Waffle.Properties;
    using Waffle.Results;

    /// <summary>Provides extension methods for <see cref="IExceptionHandler"/>.</summary>
    public static class ExceptionHandlerExtensions
    {
        /// <summary>Calls an exception handler and determines the response handling it, if any.</summary>
        /// <param name="handler">The unhandled exception handler.</param>
        /// <param name="context">The exception context.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that, when completed, contains the response message to return when the exception is handled, or
        /// <see langword="null"/> when the exception remains unhandled.
        /// </returns>
        public static Task<HandlerResponse> HandleAsync(this IExceptionHandler handler, ExceptionContext context, CancellationToken cancellationToken)
        {
            if (handler == null)
            {
                throw Error.ArgumentNull("handler");
            }

            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            ExceptionHandlerContext handlerContext = new ExceptionHandlerContext(context);
            return HandleAsyncCore(handler, handlerContext, cancellationToken);
        }

        private static async Task<HandlerResponse> HandleAsyncCore(IExceptionHandler handler, ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            Contract.Assert(handler != null);
            Contract.Assert(context != null);

            await handler.HandleAsync(context, cancellationToken);

            ICommandHandlerResult result = context.Result;

            if (result == null)
            {
                return null;
            }

            HandlerResponse response = await result.ExecuteAsync(cancellationToken);

            if (response == null)
            {
                throw Error.ArgumentNull(Resources.TypeMethodMustNotReturnNull, typeof(ICommandHandlerResult).Name, "ExecuteAsync");
            }

            return response;
        }
    }
}
