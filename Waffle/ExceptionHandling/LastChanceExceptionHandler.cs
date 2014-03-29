namespace Waffle.ExceptionHandling
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Internal;
    using Waffle.Results;
    using Waffle.Services;

    internal class LastChanceExceptionHandler : IExceptionHandler
    {
        private readonly IExceptionHandler innerHandler;

        public LastChanceExceptionHandler(IExceptionHandler innerHandler)
        {
            if (innerHandler == null)
            {
                throw Error.ArgumentNull("innerHandler");
            }

            this.innerHandler = innerHandler;
        }

        public IExceptionHandler InnerHandler
        {
            get { return this.innerHandler; }
        }

        public Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            if (context != null)
            {
                ExceptionContext exceptionContext = context.ExceptionContext;
                Contract.Assert(exceptionContext != null);

                ExceptionContextCatchBlock catchBlock = exceptionContext.CatchBlock;

                if (catchBlock != null && catchBlock.IsTopLevel)
                {
                    context.Result = CreateDefaultLastChanceResult(exceptionContext);
                }
            }

            return this.innerHandler.HandleAsync(context, cancellationToken);
        }

        private static ICommandHandlerResult CreateDefaultLastChanceResult(ExceptionContext context)
        {
            Contract.Requires(context != null);

            if (context.ExceptionInfo == null)
            {
                return null;
            }

            Exception exception = context.ExceptionInfo.SourceException;
            CommandHandlerRequest request = context.Request;

            if (request == null)
            {
                return null;
            }

            ProcessorConfiguration configuration = request.Configuration;

            if (configuration == null)
            {
                return null;
            }

            ServicesContainer services = configuration.Services;
            Contract.Assert(services != null);

            return new ExceptionResult(exception, request);
        }
    }
}
