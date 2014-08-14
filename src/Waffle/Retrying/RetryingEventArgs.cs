namespace Waffle.Retrying
{
    using System;
    using Waffle.Internal;

    /// <summary>
    /// Contains information that is required for the <see cref="RetryPolicy.Retrying" /> event.
    /// </summary>
    public class RetryingEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RetryingEventArgs" /> class.
        /// </summary>
        /// <param name="currentRetryCount">The current retry attempt count.</param>
        /// <param name="delay">The delay that indicates how long the current thread will be suspended before the next iteration is invoked.</param>
        /// <param name="lastException">The exception that caused the retry conditions to occur.</param>
        public RetryingEventArgs(int currentRetryCount, TimeSpan delay, Exception lastException)
        {
            if (lastException == null)
            {
                throw Error.ArgumentNull("lastException");
            }

            this.CurrentRetryCount = currentRetryCount;
            this.Delay = delay;
            this.LastException = lastException;
        }

        /// <summary>
        /// Gets the current retry count.
        /// </summary>
        /// <value>The current retry count.</value>
        public int CurrentRetryCount { get; private set; }

        /// <summary>
        /// Gets the delay that indicates how long the current thread will be suspended before the next iteration is invoked.
        /// </summary>
        /// <value>The delay that indicates how long the current thread will be suspended before the next iteration is invoked.</value>
        public TimeSpan Delay { get; private set; }

        /// <summary>
        /// Gets the exception that caused the retry conditions to occur.
        /// </summary>
        /// <value>The exception that caused the retry conditions to occur.</value>
        public Exception LastException { get; private set; }
    }
}