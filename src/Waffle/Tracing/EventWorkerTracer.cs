namespace Waffle.Tracing
{
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Events;

    /// <summary>
    /// Tracer for <see cref="IEventWorker"/>.
    /// </summary>
    internal class EventWorkerTracer : IEventWorker, IDecorator<IEventWorker>
    {
        private const string ExecuteMethodName = "PublishAsync";

        private readonly IEventWorker innerWorker;
        private readonly ITraceWriter traceWriter;

        public EventWorkerTracer(IEventWorker innerWorker, ITraceWriter traceWriter)
        {
            Contract.Requires(innerWorker != null);
            Contract.Requires(traceWriter != null);

            this.innerWorker = innerWorker;
            this.traceWriter = traceWriter;
        }

        public IEventWorker Inner
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

        public Task PublishAsync(EventHandlerRequest request, CancellationToken cancellationToken)
        {
            return this.TraceWriter.TraceBeginEnd(
               request,
               TraceCategories.RequestsCategory,
               TraceLevel.Info,
               this.Inner.GetType().Name,
               ExecuteMethodName,
               beginTrace: null,
               execute: () => this.Inner.PublishAsync(request, cancellationToken),
               endTrace: null,
               errorTrace: null);
        }
    }
}