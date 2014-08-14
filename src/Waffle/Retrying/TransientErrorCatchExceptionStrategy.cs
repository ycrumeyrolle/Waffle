namespace Waffle.Retrying
{
    using System;

    /// <summary>
    /// Implements a strategy that treats specific exception as transient errors.
    /// </summary>
    public sealed class TransientErrorCatchExceptionStrategy<TException> : ITransientErrorDetectionStrategy where TException : Exception
    {
        /// <summary>
        /// Determines whether the specified exception represents a transient failure that can be compensated by a retry.
        /// </summary>
        /// <param name="ex">The exception object to be verified.</param>
        /// <returns>true if the specified exception is considered as transient; otherwise, false.</returns>
        public bool IsTransient(Exception ex)
        {
            return ex is TException;
        }
    }
}