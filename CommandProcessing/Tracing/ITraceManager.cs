namespace CommandProcessing.Tracing
{
    /// <summary>
    /// Interface to initialize the tracing layer.
    /// </summary>
    /// <remarks>
    /// This is an extensibility interface that may be inserted into
    ///     <see cref="ProcessorConfiguration.Services"/> to provide a replacement for the
    ///     entire tracing layer.
    /// </remarks>
    public interface ITraceManager
    {
        /// <summary>
        /// The initialize.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        void Initialize(ProcessorConfiguration configuration);
    }
}