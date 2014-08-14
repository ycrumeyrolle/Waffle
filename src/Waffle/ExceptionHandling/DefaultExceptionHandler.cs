namespace Waffle.ExceptionHandling
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.ExceptionServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Internal;
    using Waffle.Properties;
    using Waffle.Results;
    using Waffle.Tasks;

    /// <summary>Provides the default implementation for handling exceptions within Web API.</summary>
    /// <remarks>
    /// This class preserves the legacy behavior of catch blocks and is the the default registered IExceptionHandler.
    /// This default service allows adding the IExceptionHandler service extensibility point without making any
    /// breaking changes in the default implementation.
    /// </remarks>
    internal class DefaultExceptionHandler : IExceptionHandler
    {
        public Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            Handle(context);
            return TaskHelpers.Completed();
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "We already shipped this code; avoiding even minor breaking changes in error handling.")]
        private static void Handle(ExceptionHandlerContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            ExceptionContext exceptionContext = context.ExceptionContext;
            ExceptionDispatchInfo exceptionInfo = exceptionContext.ExceptionInfo;

            if (exceptionInfo == null)
            {
                throw Error.Argument("context", Resources.TypePropertyMustNotBeNull, typeof(ExceptionContext).Name, "ExceptionInfo");
            }

            CommandHandlerRequest request = exceptionContext.Request;

            if (request == null)
            {
                throw Error.Argument("context", Resources.TypePropertyMustNotBeNull, typeof(ExceptionContext).Name, "Request");
            }

            if (exceptionContext.CatchBlock == ExceptionCatchBlocks.ExceptionFilter)
            {
                // The exception filter stage propagates unhandled exceptions by default (when no filter handles the
                // exception).
                return;
            }

            context.Result = new ResponseMessageResult(request.CreateErrorResponse(exceptionInfo.SourceException));
        }
    }
}
