namespace CommandProcessing.Filters
{
    using System;

    public class HandlerExecutedContext
    {
        public HandlerExecutedContext(HandlerExecutingContext context, bool canceled, Exception exception)
        {
            this.Context = context;
            this.Canceled = canceled;
            this.Exception = exception;
        }

        public HandlerExecutingContext Context { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:CommandProcessing.Filters.HandlerExecutedContext"/> object is canceled.
        /// </summary>
        /// <returns>
        /// true if the context canceled; otherwise, false.
        /// </returns>
        /// <value>
        /// The canceled value.
        /// </value>
        public bool Canceled { get; set; }

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
        /// True if the exception is handled; otherwise, false.
        /// </returns>
        /// <value>
        /// The exception handled value.
        /// </value>
        public bool ExceptionHandled { get; set; }

        /// <summary>
        /// Gets or sets the result returned by the action method.
        /// </summary>
        /// <returns>
        /// The result returned by the action method.
        /// </returns>
        /// <value>
        /// The <see cref="HandlerResult"/>.
        /// </value>
        public object Result { get; set; }
    }
}