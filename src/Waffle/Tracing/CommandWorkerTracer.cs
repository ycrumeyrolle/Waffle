namespace Waffle.Tracing
{
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;
    using Waffle.Commands;

    /// <summary>
    /// Tracer for <see cref="ICommandHandler"/>.
    /// </summary>
    internal class CommandWorkerTracer : ICommandWorker, IDecorator<ICommandWorker>
    {
        private const string ExecuteMethodName = "Execute";

        private readonly ICommandWorker innerWorker;
        private readonly ITraceWriter traceWriter;

        public CommandWorkerTracer(ICommandWorker innerWorker, ITraceWriter traceWriter)
        {
            Contract.Requires(innerWorker != null);
            Contract.Requires(traceWriter != null);

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

        public Task<HandlerResponse> ExecuteAsync(CommandHandlerRequest request)
        {
            return this.TraceWriter.TraceBeginEnd(
               request,
               TraceCategories.RequestsCategory,
               TraceLevel.Info,
               this.Inner.GetType().Name,
               ExecuteMethodName,
               beginTrace: null,
               execute: () => this.Inner.ExecuteAsync(request),
               endTrace: null,
               errorTrace: null);
        }
    }
}