namespace CommandProcessing.Tracing
{
    using System;

    /// <summary>
    /// Interface to write <see cref="TraceRecord"/> instances.
    /// </summary>
    public interface ITraceWriter
    {
        /// <summary>
        /// Creates and writes a new <see cref="TraceRecord"/> to the current <see cref="ITraceWriter"/>
        ///     if tracing is enabled for the given <paramref name="category"/> and <paramref name="level"/>.
        /// </summary>
        /// <remarks>
        /// The decision whether tracing is enabled for a specific category and level
        ///     is an implementation detail of each individual <see cref="ITraceWriter"/>.
        ///     <para>
        /// If the current <see cref="ITraceWriter"/> decides tracing is enabled for the given
        ///         category and level, it will construct a new <see cref="TraceRecord"/> and invoke
        ///         the caller's <paramref name="traceAction"/> to allow the caller to fill in additional
        ///         information.
        ///     </para>
        /// <para>
        /// If the current <see cref="ITraceWriter"/> decides tracing is not enabled for the given
        ///         category and level, no <see cref="TraceRecord"/> will be created,
        ///         and the <paramref name="traceAction"/> will not be called.
        ///     </para>
        /// </remarks>
        /// <param name="request">
        /// The <see cref="HandlerRequest"/> with which to associate
        ///     the <see cref="TraceRecord"/>.  It may be <c>null</c> but doing so will result in
        ///     a <see cref="TraceRecord"/> that is not correlated with its originating request.
        /// </param>
        /// <param name="category">
        /// The logical category for the trace.  Users may define their own.
        /// </param>
        /// <param name="level">
        /// The <see cref="TraceLevel"/> at which to write this trace.
        /// </param>
        /// <param name="traceAction">
        /// The action to invoke if tracing is enabled.  The caller is expected
        ///     to fill in any or all of the values of the given <see cref="TraceRecord"/> in this action.
        /// </param>
        void Trace(HandlerRequest request, string category, TraceLevel level, Action<TraceRecord> traceAction);
    }
}