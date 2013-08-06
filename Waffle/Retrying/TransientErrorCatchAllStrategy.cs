namespace Waffle.Retrying
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Implements a strategy that treats all exceptions as transient errors.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "CatchAll", Justification = "False positive.")]
    public sealed class TransientErrorCatchAllStrategy : ITransientErrorDetectionStrategy
    {
        /// <summary>
        /// Always returns true.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <returns>Always true.</returns>
        public bool IsTransient(Exception ex)
        {
            return true;
        }
    }
}