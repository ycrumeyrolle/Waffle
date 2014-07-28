namespace Waffle.ExceptionHandling
{
    using System.Diagnostics.Contracts;
    using System.Runtime.ExceptionServices;
    using Waffle.Internal;
    using Waffle.Properties;

    /// <summary>Represents the context within which unhandled exception logging occurs.</summary>
    public class ExceptionLoggerContext
    {
        private readonly ExceptionContext exceptionContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionLoggerContext"/> class using the values provided.
        /// </summary>
        /// <param name="exceptionContext">The exception context.</param>
        public ExceptionLoggerContext(ExceptionContext exceptionContext)
        {
            if (exceptionContext == null)
            {
                throw Error.ArgumentNull("exceptionContext");
            }

            this.exceptionContext = exceptionContext;
        }

        /// <summary>Gets the exception context providing the exception and related data.</summary>
        public ExceptionContext ExceptionContext
        {
            get { return this.exceptionContext; }
        }

        /// <summary>Gets the exception caught.</summary>
        public ExceptionDispatchInfo ExceptionInfo
        {
            get { return this.exceptionContext.ExceptionInfo; }
        }

        /// <summary>Gets the catch block in which the exception was caught.</summary>
        public ExceptionContextCatchBlock CatchBlock
        {
            get { return this.exceptionContext.CatchBlock; }
        }

        /// <summary>Gets the request being processed when the exception was caught.</summary>
        public HandlerRequest Request
        {
            get { return this.exceptionContext.Request; }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the exception can subsequently be handled by an
        /// <see cref="IExceptionHandler"/> to produce a new response message.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Some exceptions are caught after a response is already partially sent, which prevents sending a new
        /// response to handle the exception. In such cases, <see cref="IExceptionLogger"/> will be called to log the
        /// exception, but the <see cref="IExceptionHandler"/> will not be called.
        /// </para>
        /// <para>
        /// If this value is <see langword="true"/>, exceptions from this catch block will be provided to both
        /// <see cref="IExceptionLogger"/> and <see cref="IExceptionHandler"/>. If this value is
        /// see langword="false"/>, exceptions from this catch block cannot be handled and will only be provided to
        /// <see cref="IExceptionLogger"/>.
        /// </para>
        /// </remarks>
        public bool CallsHandler
        {
            get
            {
                Contract.Assert(this.exceptionContext != null);
                ExceptionContextCatchBlock catchBlock = this.exceptionContext.CatchBlock;

                if (catchBlock == null)
                {
                    throw Error.InvalidOperation(Resources.TypePropertyMustNotBeNull, typeof(ExceptionContext).Name, "CatchBlock");
                }

                return catchBlock.CallsHandler;
            }
        }
    }
}
