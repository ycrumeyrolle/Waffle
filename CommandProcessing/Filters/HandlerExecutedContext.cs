namespace CommandProcessing.Filters
{
    using System;
    using CommandProcessing.Internal;

    public class HandlerExecutedContext
    {
        private HandlerContext handlerContext;

        public HandlerExecutedContext(HandlerContext handlerContext, Exception exception)
        {
            if (handlerContext == null)
            {
                throw new ArgumentNullException("handlerContext");
            }

            this.Exception = exception;
            this.handlerContext = handlerContext;
        }

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

        public Exception Exception { get; set; }

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