namespace CommandProcessing.Tracing
{
    using System;

    internal static class TraceLevelHelper
    {
        private static bool IsDefined(TraceLevel traceLevel)
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
    }
}