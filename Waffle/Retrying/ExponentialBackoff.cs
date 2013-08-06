namespace Waffle.Retrying
{
    using System;
    using Waffle.Internal;

    /// <summary>
    /// A retry strategy with backOff parameters for calculating the exponential delay between retries.
    /// </summary>
    public class ExponentialBackOff : RetryStrategy
    {
        private readonly int retryCount;
        private readonly TimeSpan minBackOff;
        private readonly TimeSpan maxBackOff;
        private readonly TimeSpan deltaBackOff;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExponentialBackOff" /> class. 
        /// </summary>
        public ExponentialBackOff()
            : this(RetryStrategy.DefaultClientRetryCount, RetryStrategy.DefaultMinBackOff, RetryStrategy.DefaultMaxBackOff, RetryStrategy.DefaultClientBackOff)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExponentialBackOff" /> class with the specified retry settings.
        /// </summary>
        /// <param name="retryCount">The maximum number of retry attempts.</param>
        /// <param name="minBackOff">The minimum backOff time.</param>
        /// <param name="maxBackOff">The maximum backOff time.</param>
        /// <param name="deltaBackOff">The value that will be used to calculate a random delta in the exponential delay between retries.</param>
        public ExponentialBackOff(int retryCount, TimeSpan minBackOff, TimeSpan maxBackOff, TimeSpan deltaBackOff)
            : this(null, retryCount, minBackOff, maxBackOff, deltaBackOff, RetryStrategy.DefaultFirstFastRetry)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExponentialBackOff" /> class with the specified name and retry settings.
        /// </summary>
        /// <param name="name">The name of the retry strategy.</param>
        /// <param name="retryCount">The maximum number of retry attempts.</param>
        /// <param name="minBackOff">The minimum backOff time.</param>
        /// <param name="maxBackOff">The maximum backOff time.</param>
        /// <param name="deltaBackOff">The value that will be used to calculate a random delta in the exponential delay between retries.</param>
        public ExponentialBackOff(string name, int retryCount, TimeSpan minBackOff, TimeSpan maxBackOff, TimeSpan deltaBackOff)
            : this(name, retryCount, minBackOff, maxBackOff, deltaBackOff, RetryStrategy.DefaultFirstFastRetry)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExponentialBackOff" /> class with the specified name, retry settings, and fast retry option.
        /// </summary>
        /// <param name="name">The name of the retry strategy.</param>
        /// <param name="retryCount">The maximum number of retry attempts.</param>
        /// <param name="minBackOff">The minimum backOff time.</param>
        /// <param name="maxBackOff">The maximum backOff time.</param>
        /// <param name="deltaBackOff">The value that will be used to calculate a random delta in the exponential delay between retries.</param>
        /// <param name="firstFastRetry"><c>true</c> to immediately retry in the first attempt; otherwise, <c>false</c>. The subsequent retries will remain subject to the configured retry interval.</param>
        public ExponentialBackOff(string name, int retryCount, TimeSpan minBackOff, TimeSpan maxBackOff, TimeSpan deltaBackOff, bool firstFastRetry)
            : base(name, firstFastRetry)
        {
            if (retryCount < 0)
            {
                throw Error.ArgumentMustBeGreaterThanOrEqualTo("retryCount", retryCount, 0);
            }

            if (minBackOff.Ticks < 0)
            {
                throw Error.ArgumentMustBeGreaterThanOrEqualTo("minBackOff", minBackOff.Ticks, 0);
            }

            if (maxBackOff.Ticks < 0)
            {
                throw Error.ArgumentMustBeGreaterThanOrEqualTo("maxBackOff", maxBackOff.Ticks, 0);
            }

            if (deltaBackOff.Ticks < 0)
            {
                throw Error.ArgumentMustBeGreaterThanOrEqualTo("deltaBackOff", deltaBackOff.Ticks, 0);
            }

            if (minBackOff.TotalMilliseconds > maxBackOff.TotalMilliseconds)
            {
                throw Error.ArgumentMustBeLessThanOrEqualTo("minBackOff", minBackOff.TotalMilliseconds, maxBackOff.TotalMilliseconds);
            }

            this.retryCount = retryCount;
            this.minBackOff = minBackOff;
            this.maxBackOff = maxBackOff;
            this.deltaBackOff = deltaBackOff;
        }

        /// <summary>
        /// Returns the corresponding ShouldRetry delegate.
        /// </summary>
        /// <returns>The ShouldRetry delegate.</returns>
        public override ShouldRetry GetShouldRetry()
        {
            return delegate(int currentRetryCount, Exception lastException, out TimeSpan retryInterval)
            {
                if (currentRetryCount < this.retryCount)
                {
                    Random random = new Random();
                    int num = (int)((Math.Pow(2.0, currentRetryCount) - 1.0) * (double)random.Next((int)(this.deltaBackOff.TotalMilliseconds * 0.8), (int)(this.deltaBackOff.TotalMilliseconds * 1.2)));
                    int num2 = (int)Math.Min(this.minBackOff.TotalMilliseconds + num, this.maxBackOff.TotalMilliseconds);
                    retryInterval = TimeSpan.FromMilliseconds(num2);
                    return true;
                }

                retryInterval = TimeSpan.Zero;
                return false;
            };
        }
    }
}