namespace CommandProcessing.Filters
{
    using System;
    using CommandProcessing.Internal;
    
    /// <summary>
    /// Contains information for the executed handler.
    /// </summary>
    public class HandlerExecutedContext
    {
        private HandlerContext handlerContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerExecutedContext"/> class.
        /// </summary>
        /// <param name="handlerContext">Then handler context.</param>
        /// <param name="exception">The exception. Optionnal.</param>
        public HandlerExecutedContext(HandlerContext handlerContext, Exception exception)
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
        public HandlerContext HandlerContext
        {
            get
            {
                return this.handlerContext;
            }

            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                this.handlerContext = value;
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
                return (HandlerContext != null && HandlerContext.Request != null)
                           ? HandlerContext.Request.Command
                           : null;
            }
        }
    }
}