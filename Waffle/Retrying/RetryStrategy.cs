namespace Waffle.Retrying
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a retry strategy that determines the number of retry attempts and the interval between retries.
    /// </summary>
    public abstract class RetryStrategy
    {
        /// <summary>
        /// Represents the default number of retry attempts.
        /// </summary>
        public static readonly int DefaultClientRetryCount = 10;

        /// <summary>
        /// Represents the default amount of time used when calculating a random delta in the exponential delay between retries.
        /// </summary>
        public static readonly TimeSpan DefaultClientBackOff = TimeSpan.FromSeconds(10.0);

        /// <summary>
        /// Represents the default maximum amount of time used when calculating the exponential delay between retries.
        /// </summary>
        public static readonly TimeSpan DefaultMaxBackOff = TimeSpan.FromSeconds(30.0);

        /// <summary>
        /// Represents the default minimum amount of time used when calculating the exponential delay between retries.
        /// </summary>
        public static readonly TimeSpan DefaultMinBackOff = TimeSpan.FromSeconds(1.0);

        /// <summary>
        /// Represents the default interval between retries.
        /// </summary>
        public static readonly TimeSpan DefaultRetryInterval = TimeSpan.FromSeconds(1.0);

        /// <summary>
        /// Represents the default time increment between retry attempts in the progressive delay policy.
        /// </summary>
        public static readonly TimeSpan DefaultRetryIncrement = TimeSpan.FromSeconds(1.0);

        /// <summary>
        /// Represents the default flag indicating whether the first retry attempt will be made immediately,
        /// whereas subsequent retries will remain subject to the retry interval.
        /// </summary>
        public static readonly bool DefaultFirstFastRetry = true;
        private static readonly RetryStrategy noRetry = new FixedInterval(0, RetryStrategy.DefaultRetryInterval);
        private static readonly RetryStrategy defaultFixed = new FixedInterval(RetryStrategy.DefaultClientRetryCount, RetryStrategy.DefaultRetryInterval);
        private static readonly RetryStrategy defaultProgressive = new Incremental(RetryStrategy.DefaultClientRetryCount, RetryStrategy.DefaultRetryInterval, RetryStrategy.DefaultRetryIncrement);
        private static readonly RetryStrategy defaultExponential = new ExponentialBackOff(RetryStrategy.DefaultClientRetryCount, RetryStrategy.DefaultMinBackOff, RetryStrategy.DefaultMaxBackOff, RetryStrategy.DefaultClientBackOff);
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RetryStrategy" /> class. 
        /// </summary>
        /// <param name="name">The name of the retry strategy.</param>
        /// <param name="firstFastRetry"><c>true</c> to immediately retry in the first attempt; otherwise, <c>false</c>. The subsequent retries will remain subject to the configured retry interval.</param>
        protected RetryStrategy(string name, bool firstFastRetry)
        {
            this.Name = name;
            this.FastFirstRetry = firstFastRetry;
        }

        /// <summary>
        /// Returns a default policy that performs no retries, but invokes the action only once.
        /// </summary>
        public static RetryStrategy NoRetry
        {
            get
            {
                return RetryStrategy.noRetry;
            }
        }

        /// <summary>
        /// Returns a default policy that implements a fixed retry interval configured with the <see cref="RetryStrategy.DefaultClientRetryCount" /> and <see cref="RetryStrategy.DefaultRetryInterval" /> parameters.
        /// The default retry policy treats all caught exceptions as transient errors.
        /// </summary>
        public static RetryStrategy DefaultFixed
        {
            get
            {
                return RetryStrategy.defaultFixed;
            }
        }

        /// <summary>
        /// Returns a default policy that implements a progressive retry interval configured with the <see cref="RetryStrategy.DefaultClientRetryCount" />, <see cref="RetryStrategy.DefaultRetryInterval" />, and <see cref="RetryStrategy.DefaultRetryIncrement" /> parameters.
        /// The default retry policy treats all caught exceptions as transient errors.
        /// </summary>
        public static RetryStrategy DefaultProgressive
        {
            get
            {
                return RetryStrategy.defaultProgressive;
            }
        }

        /// <summary>
        /// Returns a default policy that implements a random exponential retry interval configured with the <see cref="RetryStrategy.DefaultClientRetryCount" />, <see cref="RetryStrategy.DefaultMinBackOff" />, <see cref="RetryStrategy.DefaultMaxBackOff" />, and <see cref="RetryStrategy.DefaultClientBackOff" /> parameters.
        /// The default retry policy treats all caught exceptions as transient errors.
        /// </summary>
        public static RetryStrategy DefaultExponential
        {
            get
            {
                return RetryStrategy.defaultExponential;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the first retry attempt will be made immediately,
        /// whereas subsequent retries will remain subject to the retry interval.
        /// </summary>
        public bool FastFirstRetry { get; set; }

        /// <summary>
        /// Gets the name of the retry strategy.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Returns the corresponding ShouldRetry delegate.
        /// </summary>
        /// <returns>The ShouldRetry delegate.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Calling the method two times in succession creates different results.")]
        public abstract ShouldRetry GetShouldRetry();
    }
}