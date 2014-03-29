namespace Waffle.ExceptionHandling
{
    using System;
    using System.Runtime.ExceptionServices;
    using Waffle.Results;

    /// <summary>Represents the context within which unhandled exception handling occurs.</summary>
    public class ExceptionHandlerContext
    {
        private readonly ExceptionContext exceptionContext;

        /// <summary>Initializes a new instance of the <see cref="ExceptionHandlerContext"/> class.</summary>
        /// <param name="exceptionContext">The exception context.</param>
        public ExceptionHandlerContext(ExceptionContext exceptionContext)
        {
            if (exceptionContext == null)
            {
                throw new ArgumentNullException("exceptionContext");
            }

            this.exceptionContext = exceptionContext;
        }

        /// <summary>Gets the exception context providing the exception and related data.</summary>
        public ExceptionContext ExceptionContext
        {
            get { return this.exceptionContext; }
        }

        /// <summary>Gets or sets the result providing the response message when the exception is handled.</summary>
        /// <remarks>
        /// If this value is <see langword="null"/>, the exception is left unhandled and will be re-thrown.
        /// </remarks>
        public ICommandHandlerResult Result { get; set; }

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
    }
}
