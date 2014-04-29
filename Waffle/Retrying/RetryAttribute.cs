namespace Waffle.Retrying
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Waffle.Commands;

    /// <summary>
    /// Represents an attribute to mark a <see cref="ICommandHandler"/> as retriable.
    /// The handler will be retried as long as the policy allow it.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "Mandatories arguments are not retrievable.")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class RetryAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RetryAttribute"/> class.
        /// </summary>
        /// <param name="retryPolicy">The <see cref="RetryPolicy"/>.</param>
        public RetryAttribute(RetryPolicy retryPolicy)
        {
            this.RetryPolicy = retryPolicy;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryAttribute"/> class with the specified number of retry attempts.
        /// </summary>
        /// <param name="retryCount">The number of retry attempts.</param>
        public RetryAttribute(int retryCount) :
            this(new TransientErrorCatchAllStrategy(), retryCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryAttribute"/> class with the specified number of retry attempts and fixed time interval between retries.
        /// </summary>        
        /// <param name="retryCount">The number of retry attempts.</param>
        /// <param name="retryInterval">The interval between retries, in milliseconds.</param>
        public RetryAttribute(int retryCount, double retryInterval) :
            this(new TransientErrorCatchAllStrategy(), retryCount, retryInterval)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryAttribute"/> class with the specified number of retry attempts and backOff parameters for calculating the exponential delay between retries.
        /// </summary>
        /// <param name="retryCount">The number of retry attempts.</param>
        /// <param name="minBackOff">The minimum backOff time. In milliseconds.</param>
        /// <param name="maxBackOff">The maximum backOff time. In milliseconds.</param>
        /// <param name="deltaBackOff">The time value that will be used to calculate a random delta in the exponential delay between retries. In milliseconds.</param>
        public RetryAttribute(int retryCount, double minBackOff, double maxBackOff, double deltaBackOff) :
            this(new TransientErrorCatchAllStrategy(), retryCount, minBackOff, maxBackOff, deltaBackOff)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryAttribute"/> class with the specified number of retry attempts and parameters defining the progressive delay between retries.
        /// </summary>
        /// <param name="retryCount">The number of retry attempts.</param>
        /// <param name="initialInterval">The initial interval that will apply for the first retry. In milliseconds.</param>
        /// <param name="increment">The incremental time value that will be used to calculate the progressive delay between retries. In milliseconds.</param>
        public RetryAttribute(int retryCount, double initialInterval, double increment) :
            this(new TransientErrorCatchAllStrategy(), retryCount, initialInterval, increment)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryAttribute"/> class with the specified number of retry attempts.
        /// </summary>
        /// <param name="errorDetectionStrategy">The <see cref="ITransientErrorDetectionStrategy" /> that is responsible for detecting transient conditions.</param>
        /// <param name="retryCount">The number of retry attempts.</param>
        public RetryAttribute(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount) :
            this(new RetryPolicy(errorDetectionStrategy, retryCount))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryAttribute"/> class with the specified number of retry attempts and fixed time interval between retries.
        /// </summary>        
        /// <param name="errorDetectionStrategy">The <see cref="ITransientErrorDetectionStrategy" /> that is responsible for detecting transient conditions.</param>
        /// <param name="retryCount">The number of retry attempts.</param>
        /// <param name="retryInterval">The interval between retries. In milliseconds.</param>
        public RetryAttribute(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, double retryInterval) :
            this(new RetryPolicy(errorDetectionStrategy, retryCount, TimeSpan.FromMilliseconds(retryInterval)))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryAttribute"/> class with the specified number of retry attempts and backOff parameters for calculating the exponential delay between retries.
        /// </summary>
        /// <param name="errorDetectionStrategy">The <see cref="ITransientErrorDetectionStrategy" /> that is responsible for detecting transient conditions.</param>
        /// <param name="retryCount">The number of retry attempts.</param>
        /// <param name="minBackOff">The minimum backOff time. In milliseconds.</param>
        /// <param name="maxBackOff">The maximum backOff time. In milliseconds.</param>
        /// <param name="deltaBackOff">The time value that will be used to calculate a random delta in the exponential delay between retries. In milliseconds.</param>
        public RetryAttribute(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, double minBackOff, double maxBackOff, double deltaBackOff) :
            this(new RetryPolicy(errorDetectionStrategy, retryCount, TimeSpan.FromMilliseconds(minBackOff), TimeSpan.FromMilliseconds(maxBackOff), TimeSpan.FromMilliseconds(deltaBackOff)))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryAttribute"/> class with the specified number of retry attempts and parameters defining the progressive delay between retries.
        /// </summary>
        /// <param name="errorDetectionStrategy">The <see cref="ITransientErrorDetectionStrategy" /> that is responsible for detecting transient conditions.</param>
        /// <param name="retryCount">The number of retry attempts.</param>
        /// <param name="initialInterval">The initial interval that will apply for the first retry. In milliseconds.</param>
        /// <param name="increment">The incremental time value that will be used to calculate the progressive delay between retries. In milliseconds.</param>
        public RetryAttribute(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, double initialInterval, double increment) :
            this(new RetryPolicy(errorDetectionStrategy, retryCount, TimeSpan.FromMilliseconds(initialInterval), TimeSpan.FromMilliseconds(increment)))
        {
        }

        /// <summary>
        /// Gets the <see cref="RetryPolicy"/>.
        /// </summary>
        public RetryPolicy RetryPolicy { get; private set; }
    }
}
