namespace Waffle.Retrying
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "Mandatories arguments are not retrievable.")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class RetryAttribute : Attribute
    {
        public RetryAttribute(RetryPolicy retryPolicy)
        {
            this.RetryPolicy = retryPolicy;
        }

        public RetryAttribute(int retryCount) :
            this(new TransientErrorCatchAllStrategy(), retryCount)
        {
        }

        public RetryAttribute(int retryCount, TimeSpan retryInterval) :
            this(new TransientErrorCatchAllStrategy(), retryCount, retryInterval)
        {
        }

        public RetryAttribute(int retryCount, TimeSpan minBackOff, TimeSpan maxBackOff, TimeSpan deltaBackOff) :
            this(new TransientErrorCatchAllStrategy(), retryCount, minBackOff, maxBackOff, deltaBackOff)
        {
        }

        public RetryAttribute(int retryCount, TimeSpan initialInterval, TimeSpan increment) :
            this(new TransientErrorCatchAllStrategy(), retryCount, initialInterval, increment)
        {
        }

        public RetryAttribute(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount) :
            this(new RetryPolicy(errorDetectionStrategy, retryCount))
        {
        }

        public RetryAttribute(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, TimeSpan retryInterval) :
            this(new RetryPolicy(errorDetectionStrategy, retryCount, retryInterval))
        {
        }

        public RetryAttribute(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, TimeSpan minBackOff, TimeSpan maxBackOff, TimeSpan deltaBackOff) :
            this(new RetryPolicy(errorDetectionStrategy, retryCount, minBackOff, maxBackOff, deltaBackOff))
        {
        }

        public RetryAttribute(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, TimeSpan initialInterval, TimeSpan increment) :
            this(new RetryPolicy(errorDetectionStrategy, retryCount, initialInterval, increment))
        {
        }

        public RetryPolicy RetryPolicy { get; private set; }
    }
}
