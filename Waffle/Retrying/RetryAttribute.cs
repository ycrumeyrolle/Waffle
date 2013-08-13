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
        /// <param name="retryInterval">The interval between retries.</param>
        public RetryAttribute(int retryCount, TimeSpan retryInterval) :
            this(new TransientErrorCatchAllStrategy(), retryCount, retryInterval)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryAttribute"/> class with the specified number of retry attempts and backOff parameters for calculating the exponential delay between retries.
        /// </summary>
        /// <param name="retryCount">The number of retry attempts.</param>
        /// <param name="minBackOff">The minimum backOff time.</param>
        /// <param name="maxBackOff">The maximum backOff time.</param>
        /// <param name="deltaBackOff">The time value that will be used to calculate a random delta in the exponential delay between retries.</param>
        public RetryAttribute(int retryCount, TimeSpan minBackOff, TimeSpan maxBackOff, TimeSpan deltaBackOff) :
            this(new TransientErrorCatchAllStrategy(), retryCount, minBackOff, maxBackOff, deltaBackOff)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryAttribute"/> class with the specified number of retry attempts and parameters defining the progressive delay between retries.
        /// </summary>
        /// <param name="retryCount">The number of retry attempts.</param>
        /// <param name="initialInterval">The initial interval that will apply for the first retry.</param>
        /// <param name="increment">The incremental time value that will be used to calculate the progressive delay between retries.</param>
        public RetryAttribute(int retryCount, TimeSpan initialInterval, TimeSpan increment) :
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
        /// <param name="retryInterval">The interval between retries.</param>
        public RetryAttribute(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, TimeSpan retryInterval) :
            this(new RetryPolicy(errorDetectionStrategy, retryCount, retryInterval))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryAttribute"/> class with the specified number of retry attempts and backOff parameters for calculating the exponential delay between retries.
        /// </summary>
        /// <param name="errorDetectionStrategy">The <see cref="ITransientErrorDetectionStrategy" /> that is responsible for detecting transient conditions.</param>
        /// <param name="retryCount">The number of retry attempts.</param>
        /// <param name="minBackOff">The minimum backOff time.</param>
        /// <param name="maxBackOff">The maximum backOff time.</param>
        /// <param name="deltaBackOff">The time value that will be used to calculate a random delta in the exponential delay between retries.</param>
        public RetryAttribute(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, TimeSpan minBackOff, TimeSpan maxBackOff, TimeSpan deltaBackOff) :
            this(new RetryPolicy(errorDetectionStrategy, retryCount, minBackOff, maxBackOff, deltaBackOff))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryAttribute"/> class with the specified number of retry attempts and parameters defining the progressive delay between retries.
        /// </summary>
        /// <param name="errorDetectionStrategy">The <see cref="ITransientErrorDetectionStrategy" /> that is responsible for detecting transient conditions.</param>
        /// <param name="retryCount">The number of retry attempts.</param>
        /// <param name="initialInterval">The initial interval that will apply for the first retry.</param>
        /// <param name="increment">The incremental time value that will be used to calculate the progressive delay between retries.</param>
        public RetryAttribute(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, TimeSpan initialInterval, TimeSpan increment) :
            this(new RetryPolicy(errorDetectionStrategy, retryCount, initialInterval, increment))
        {
        }

        /// <summary>
        /// Gets the <see cref="RetryPolicy"/>.
        /// </summary>
        public RetryPolicy RetryPolicy { get; private set; }
    }
}
