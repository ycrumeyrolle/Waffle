namespace CommandProcessing.Tracing
{
    /// <summary>
    /// Describes the kind of <see cref="TraceRecord"/> for an individual trace operation.
    /// </summary>
    public enum TraceKind
    {
        /// <summary>
        /// Single trace, not part of a Begin/End trace pair.
        /// </summary>
        Trace = 0, 

        /// <summary>
        /// Trace marking the beginning of some operation.
        /// </summary>
        Begin, 

        /// <summary>
        /// Trace marking the end of some operation.
        /// </summary>
        End, 
    }
}