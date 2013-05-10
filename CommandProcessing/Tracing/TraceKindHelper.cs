namespace CommandProcessing.Tracing
{
    using System;

    internal static class TraceKindHelper
    {
        private static bool IsDefined(TraceKind traceKind)
        {
            return traceKind == TraceKind.Trace || traceKind == TraceKind.Begin || traceKind == TraceKind.End;
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
        public static void Validate(TraceKind value, string parameterValue)
        {
            if (!IsDefined(value))
            {
                throw Internal.Error.InvalidEnumArgument(parameterValue, (int)value, typeof(TraceKind));
            }
        }
    }
}