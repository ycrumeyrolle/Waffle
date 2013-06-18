namespace CommandProcessing.Tracing
{
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Tracer for <see cref="IHandler"/>.
    /// </summary>
    internal class CommandWorkerTracer : ICommandWorker, IDecorator<ICommandWorker>
    {
        private const string ExecuteMethodName = "Execute";

        private readonly ICommandWorker innerWorker;
        private readonly ITraceWriter traceWriter;

        public CommandWorkerTracer(ICommandWorker innerWorker, ITraceWriter traceWriter)
        {
            Contract.Assert(innerWorker != null);
            Contract.Assert(traceWriter != null);

            this.innerWorker = innerWorker;
            this.traceWriter = traceWriter;
        }

        public ICommandWorker Inner
        {
            get { return this.innerWorker; }
        }
        
        public ITraceWriter TraceWriter
        {
            get
            {
                return this.traceWriter;
            }
        }

        public TResult Execute<TResult>(HandlerRequest request)
        {
            return this.TraceWriter.TraceBeginEnd(
               request,
               TraceCategories.RequestsCategory,
               TraceLevel.Info,
               this.Inner.GetType().Name,
               ExecuteMethodName,
               beginTrace: null,
               execute: () => this.Inner.Execute<TResult>(request),
               endTrace: null,
               errorTrace: null);
        }
    }
}