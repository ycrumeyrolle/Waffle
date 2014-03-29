namespace Waffle.ExceptionHandling
{
    using System;
    using System.Runtime.ExceptionServices;
    using Waffle.Commands;
    using Waffle.Filters;
    using Waffle.Internal;
    using Waffle.Properties;

    /// <summary>Represents an exception and the contextual data associated with it when it was caught.</summary>
    public class ExceptionContext
    {
        /// <summary>Initializes a new instance of the <see cref="ExceptionContext"/> class.</summary>
        /// <remarks>This constructor is for unit testing purposes only.</remarks>
        public ExceptionContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionContext"/> class using the values provided.
        /// </summary>
        /// <param name="exceptionInfo">The exception caught.</param>
        /// <param name="catchBlock">The catch block where the exception was caught.</param>
        /// <param name="context">The context in which the exception occurred.</param>
        public ExceptionContext(ExceptionDispatchInfo exceptionInfo, ExceptionContextCatchBlock catchBlock, CommandHandlerContext context)
        {
            if (exceptionInfo == null)
            {
                throw new ArgumentNullException("exceptionInfo");
            }

            this.ExceptionInfo = exceptionInfo;

            if (catchBlock == null)
            {
                throw new ArgumentNullException("catchBlock");
            }

            this.CatchBlock = catchBlock;

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            this.Context = context;

            CommandHandlerRequest request = context.Request;

            if (request == null)
            {
                throw Error.ArgumentNull(Resources.TypePropertyMustNotBeNull, typeof(HandlerRequest).Name, "Request", "context");
            }

            this.Request = request;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionContext"/> class using the values provided.
        /// </summary>
        /// <param name="exceptionInfo">The exception caught.</param>
        /// <param name="catchBlock">The catch block where the exception was caught.</param>
        /// <param name="request">The request being processed when the exception was caught.</param>
        public ExceptionContext(ExceptionDispatchInfo exceptionInfo, ExceptionContextCatchBlock catchBlock, CommandHandlerRequest request)
        {
            if (exceptionInfo == null)
            {
                throw new ArgumentNullException("exceptionInfo");
            }

            this.ExceptionInfo = exceptionInfo;

            if (catchBlock == null)
            {
                throw new ArgumentNullException("catchBlock");
            }

            this.CatchBlock = catchBlock;

            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            this.Request = request;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionContext"/> class using the values provided.
        /// </summary>
        /// <param name="exceptionInfo">The exception caught.</param>
        /// <param name="catchBlock">The catch block where the exception was caught.</param>
        /// <param name="request">The request being processed when the exception was caught.</param>
        /// <param name="response">The repsonse being returned when the exception was caught.</param>
        public ExceptionContext(ExceptionDispatchInfo exceptionInfo, ExceptionContextCatchBlock catchBlock, CommandHandlerRequest request, HandlerResponse response)
        {
            if (exceptionInfo == null)
            {
                throw new ArgumentNullException("exceptionInfo");
            }

            this.ExceptionInfo = exceptionInfo;

            if (catchBlock == null)
            {
                throw new ArgumentNullException("catchBlock");
            }

            this.CatchBlock = catchBlock;

            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            this.Request = request;

            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            this.Response = response;
        }

        /// <summary>Gets the exception caught.</summary>
        /// <remarks>The setter is for unit testing purposes only.</remarks>
        public ExceptionDispatchInfo ExceptionInfo { get; set; }

        /// <summary>Gets the catch block in which the exception was caught.</summary>
        /// <remarks>The setter is for unit testing purposes only.</remarks>
        public ExceptionContextCatchBlock CatchBlock { get; set; }

        /// <summary>Gets the request being processed when the exception was caught.</summary>
        /// <remarks>The setter is for unit testing purposes only.</remarks>
        public CommandHandlerRequest Request { get; set; }

        /// <summary>Gets the handler context in which the exception occurred, if available.</summary>
        /// <remarks>
        /// <para>This property will be <see langword="null"/> in most cases.</para>
        /// <para>The setter is for unit testing purposes only.</para>
        /// </remarks>
        public CommandHandlerContext Context { get; set; }

        /// <summary>Gets the response being sent when the exception was caught.</summary>
        /// <remarks>
        /// <para>This property will be <see langword="null"/> in most cases.</para>
        /// <para>The setter is for unit testing purposes only.</para>
        /// </remarks>
        public HandlerResponse Response { get; set; }
    }
}
