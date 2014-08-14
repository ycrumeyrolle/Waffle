namespace Waffle.MiniProfiler
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using StackExchange.Profiling;
    using Waffle.Tracing;

    /// <summary>
    /// Represents the tracer for MiniProfiler.
    /// </summary>
    public class PerformanceTracer : ITraceWriter
    {
        private readonly ConcurrentDictionary<TraceRecord, IDisposable> bag = new ConcurrentDictionary<TraceRecord, IDisposable>(new TraceRecordComparer());

        /// <summary>
        /// Creates and writes a new <see cref="TraceRecord"/> to the current <see cref="ITraceWriter"/>
        ///     if tracing is enabled for the given <paramref name="category"/> and <paramref name="level"/>.
        /// </summary>
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
        public void Trace(HandlerRequest request, string category, TraceLevel level, Action<TraceRecord> traceAction)
        {
            if (traceAction == null)
            {
                throw new ArgumentNullException("traceAction");
            }

            TraceRecord record = new TraceRecord(request, category, level);
            traceAction(record);
            this.WriteTrace(record);
        }

        // Collect traces in memory.
        private void WriteTrace(TraceRecord record)
        {
            switch (record.Kind)
            {
                case TraceKind.Begin:
                    this.HandleBeginTrace(record);
                    break;

                case TraceKind.End:
                    this.HandleEndTrace(record);
                    break;
            }
        }

        private void HandleBeginTrace(TraceRecord record)
        {
            MiniProfiler profiler = MiniProfiler.Current;
            if (profiler != null)
            {
                IDisposable step = profiler.Step(record.Message);
                this.bag.TryAdd(record, step);
            }
        }

        private void HandleEndTrace(TraceRecord record)
        {
            IDisposable step;
            if (this.bag.TryRemove(record, out step))
            {
                step.Dispose();
            }
            else
            {
                MiniProfiler profiler = MiniProfiler.Current;
                if (profiler != null)
                {
                    using (profiler.Step(record.Message))
                    {
                    }
                }
            }
        }

        private class TraceRecordComparer : IEqualityComparer<TraceRecord>
        {
            public bool Equals(TraceRecord x, TraceRecord y)
            {
                if (x == null && y == null)
                {
                    return true;
                }

                if (x == null)
                {
                    return false;
                }

                if (y == null)
                {
                    return false;
                }

                return x.RequestId == y.RequestId && x.Category == y.Category && x.Operation == y.Operation && x.Operator == y.Operator;
            }

            public int GetHashCode(TraceRecord obj)
            {
                if (obj == null)
                {
                    return 0;
                }

                return obj.RequestId.GetHashCode() 
                    ^ (obj.Category ?? string.Empty).GetHashCode() 
                    ^ (obj.Operation ?? string.Empty).GetHashCode() 
                    ^ (obj.Operator ?? string.Empty).GetHashCode();
            }
        }
    }
}
