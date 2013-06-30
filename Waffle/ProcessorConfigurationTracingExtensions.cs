namespace Waffle
{
    using System;
    using System.ComponentModel;
    using Waffle.Internal;
    using Waffle.Tracing;

    /// <summary>
    /// This static class contains helper methods related to the registration
    /// of <see cref="ITraceWriter"/> instances.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ProcessorConfigurationTracingExtensions
    {
        /// <summary>
        /// Creates and registers an <see cref="ITraceWriter"/> implementation to use
        /// for this application.
        /// </summary>
        /// <param name="configuration">The <see cref="ProcessorConfiguration"/> for which
        /// to register the created trace writer.</param>
        /// <remarks>The returned SystemDiagnosticsTraceWriter may be further configured to change it's default settings.</remarks>
        /// <returns>The <see cref="DefaultTraceWriter"/> which was created and registered.</returns>
        public static DefaultTraceWriter EnableDefaultTracing(this ProcessorConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            DefaultTraceWriter traceWriter = new DefaultTraceWriter
                                                {
                                                    MinimumLevel = TraceLevel.Info,
                                                    IsVerbose = false
                                                };

            configuration.Services.Replace(typeof(ITraceWriter), traceWriter);

            return traceWriter;
        }
    }
}