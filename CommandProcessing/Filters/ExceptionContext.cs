namespace CommandProcessing.Filters
{
    using System;

    public class ExceptionContext : HandlerContext
    {
        private HandlerResult result;

        public ExceptionContext(HandlerContext context, Exception exception)
            : base(context)
        {
            this.Exception = exception;
        }

        /// <summary>
        /// Gets or sets the exception that occurred during the execution of the action method, if any.
        /// </summary>
        /// <returns>
        /// The exception that occurred during the execution of the action method.
        /// </returns>
        /// <value>
        /// The exception.
        /// </value>
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the exception is handled.
        /// </summary>
        /// <returns>
        /// true if the exception is handled; otherwise, false.
        /// </returns>
        /// <value>
        /// The exception handled.
        /// </value>
        public bool ExceptionHandled { get; set; }

        /// <summary>
        /// Gets or sets the result returned by the action method.
        /// </summary>
        /// <returns>
        /// The result returned by the action method.
        /// </returns>
        /// <value>
        /// The result.
        /// </value>
        public HandlerResult Result
        {
            get
            {
                return this.result ?? EmptyResult.Instance;
            }

            set
            {
                this.result = value;
            }
        }
    }
}