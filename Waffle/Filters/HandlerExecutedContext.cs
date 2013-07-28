namespace Waffle.Filters
{
    using System;
    using Waffle.Commands;
    using Waffle.Internal;

    /// <summary>
    /// Contains information for the executed handler.
    /// </summary>
    public class HandlerExecutedContext
    {
        private readonly CommandHandlerContext handlerContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerExecutedContext"/> class.
        /// </summary>
        /// <param name="handlerContext">Then handler context.</param>
        /// <param name="exception">The exception. Optionnal.</param>
        public HandlerExecutedContext(CommandHandlerContext handlerContext, Exception exception)
        {
            if (handlerContext == null)
            {
                throw Error.ArgumentNull("handlerContext");
            }

            this.Exception = exception;
            this.handlerContext = handlerContext;
        }

        /// <summary>
        /// Gets or sets the handler context. 
        /// </summary>
        /// <value>The handler context.</value>
        public CommandHandlerContext HandlerContext
        {
            get
            {
                return this.handlerContext;
            }
        }

        /// <summary>
        /// Gets or sets the exception that was raised during the execution.
        /// </summary>
        /// <value>The exception that was raised during the execution.</value>
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets or sets the handler result.
        /// </summary>
        /// <value>The handler result.</value>
        public object Result
        {
            get
            {
                return this.HandlerContext != null ? this.HandlerContext.Result : null;
            }

            set
            {
                this.HandlerContext.Result = value;
            }
        }

        /// <summary>
        /// Gets the current <see cref="ICommand"/>.
        /// </summary>
        /// <value>
        /// The <see cref="ICommand"/>.
        /// </value>
        public ICommand Command
        {
            get
            {
                return (this.HandlerContext != null && this.HandlerContext.Request != null)
                           ? this.HandlerContext.Request.Command
                           : null;
            }
        }

        /// <summary>
        /// Gets the current <see cref="HandlerRequest"/>.
        /// </summary>
        /// <value>
        /// The <see cref="HandlerRequest"/>.
        /// </value>
        public HandlerRequest Request
        {
            get
            {
                return (this.HandlerContext != null && this.HandlerContext.Request != null)
                           ? this.HandlerContext.Request
                           : null;
            }
        }
    }
}