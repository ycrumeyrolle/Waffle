namespace Waffle.Retrying
{
    using System;
    using Waffle.Internal;

    /// <summary>
    /// A retry strategy with a specified number of retry attempts and an incremental time interval between retries.
    /// </summary>
    public class Incremental : RetryStrategy
    {
        private readonly int retryCount;
        private readonly TimeSpan initialInterval;
        private readonly TimeSpan increment;

        /// <summary>
        /// Initializes a new instance of the <see cref="Incremental" /> class. 
        /// </summary>
        public Incremental()
            : this(RetryStrategy.DefaultClientRetryCount, RetryStrategy.DefaultRetryInterval, RetryStrategy.DefaultRetryIncrement)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Incremental" /> class with the specified retry settings.
        /// </summary>
        /// <param name="retryCount">The number of retry attempts.</param>
        /// <param name="initialInterval">The initial interval that will apply for the first retry.</param>
        /// <param name="increment">The incremental time value that will be used to calculate the progressive delay between retries.</param>
        public Incremental(int retryCount, TimeSpan initialInterval, TimeSpan increment)
            : this(null, retryCount, initialInterval, increment)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Incremental" /> class with the specified name and retry settings.
        /// </summary>
        /// <param name="name">The retry strategy name.</param>
        /// <param name="retryCount">The number of retry attempts.</param>
        /// <param name="initialInterval">The initial interval that will apply for the first retry.</param>
        /// <param name="increment">The incremental time value that will be used to calculate the progressive delay between retries.</param>
        public Incremental(string name, int retryCount, TimeSpan initialInterval, TimeSpan increment)
            : this(name, retryCount, initialInterval, increment, RetryStrategy.DefaultFirstFastRetry)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Incremental" /> class with the specified number of retry attempts, time interval, retry strategy, and fast start option. 
        /// </summary>
        /// <param name="name">The retry strategy name.</param>
        /// <param name="retryCount">The number of retry attempts.</param>
        /// <param name="initialInterval">The initial interval that will apply for the first retry.</param>
        /// <param name="increment">The incremental time value that will be used to calculate the progressive delay between retries.</param>
        /// <param name="firstFastRetry"><see langword="true"/> to immediately retry in the first attempt; otherwise, <see langword="false"/>. The subsequent retries will remain subject to the configured retry interval.</param>
        public Incremental(string name, int retryCount, TimeSpan initialInterval, TimeSpan increment, bool firstFastRetry)
            : base(name, firstFastRetry)
        {
            if (retryCount < 0)
            {
                throw Error.ArgumentMustBeGreaterThanOrEqualTo("retryCount", retryCount, 0);
            }

            if (initialInterval.Ticks < 0)
            {
                throw Error.ArgumentMustBeGreaterThanOrEqualTo("initialInterval", initialInterval.Ticks, 0);
            }

            if (increment.Ticks < 0)
            {
                throw Error.ArgumentMustBeGreaterThanOrEqualTo("increment", increment.Ticks, 0);
            }

            this.retryCount = retryCount;
            this.initialInterval = initialInterval;
            this.increment = increment;
        }

        /// <summary>
        /// Returns the corresponding ShouldRetry delegate.
        /// </summary>
        /// <returns>The ShouldRetry delegate.</returns>
        public override ShouldRetry GetShouldRetry()
        {
            return (int currentRetryCount, Exception lastException, out TimeSpan retryInterval) =>
            {
                if (currentRetryCount < this.retryCount)
                {
                    retryInterval = TimeSpan.FromMilliseconds(this.initialInterval.TotalMilliseconds + (this.increment.TotalMilliseconds * currentRetryCount));
                    return true;
                }

                retryInterval = TimeSpan.Zero;
                return false;
            };
        }
    }
}