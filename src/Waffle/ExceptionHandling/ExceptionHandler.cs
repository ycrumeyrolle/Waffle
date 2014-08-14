namespace Waffle.ExceptionHandling
{
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Internal;
    using Waffle.Properties;
    using Waffle.Tasks;

    /// <summary>Represents an unhandled exception handler.</summary>
    public abstract class ExceptionHandler : IExceptionHandler
    {
        /// <inheritdoc />
        Task IExceptionHandler.HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            ExceptionContext exceptionContext = context.ExceptionContext;

            if (exceptionContext.ExceptionInfo == null)
            {
                throw Error.Argument("context", Resources.TypePropertyMustNotBeNull, typeof(ExceptionContext).Name, "ExceptionInfo");
            }

            if (!this.ShouldHandle(context))
            {
                return TaskHelpers.Completed();
            }

            return this.HandleAsync(context, cancellationToken);
        }

        /// <summary>When overridden in a derived class, handles the exception asynchronously.</summary>
        /// <param name="context">The exception handler context.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous exception handling operation.</returns>
        public virtual Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            this.Handle(context);
            return TaskHelpers.Completed();
        }

        /// <summary>When overridden in a derived class, handles the exception synchronously.</summary>
        /// <param name="context">The exception handler context.</param>
        public virtual void Handle(ExceptionHandlerContext context)
        {
        }

        /// <summary>Determines whether the exception should be handled.</summary>
        /// <param name="context">The exception handler context.</param>
        /// <returns>
        /// <see langword="true"/> if the exception should be handled; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>The default decision is only to handle exceptions caught at top-level catch blocks.</remarks>
        public virtual bool ShouldHandle(ExceptionHandlerContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            ExceptionContext exceptionContext = context.ExceptionContext;

            ExceptionContextCatchBlock catchBlock = exceptionContext.CatchBlock;

            if (catchBlock == null)
            {
                throw Error.ArgumentNull("context", Resources.TypePropertyMustNotBeNull, typeof(ExceptionContext), "CatchBlock");
            }

            return catchBlock.IsTopLevel;
        }
    }
}
