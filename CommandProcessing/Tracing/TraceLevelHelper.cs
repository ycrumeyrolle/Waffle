namespace CommandProcessing.Tracing
{
    using System;

    internal static class TraceLevelHelper
    {
        #region Public Methods and Operators

        /// <summary>
        /// The is defined.
        /// </summary>
        /// <param name="traceLevel">
        /// The trace level.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsDefined(TraceLevel traceLevel)
        {
            return traceLevel == TraceLevel.Off || traceLevel == TraceLevel.Debug || traceLevel == TraceLevel.Info || traceLevel == TraceLevel.Warn || traceLevel == TraceLevel.Error || traceLevel == TraceLevel.Fatal;
        }

        /// <summary>
        /// The validate.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="parameterValue">
        /// The parameter value.
        /// </param>
        /// <exception cref="ArgumentException">
        /// </exception>
        public static void Validate(TraceLevel value, string parameterValue)
        {
            if (!IsDefined(value))
            {
                throw Internal.Error.InvalidEnumArgument(parameterValue, (int)value, typeof(TraceLevel));
            }
        }

        #endregion
    }
}