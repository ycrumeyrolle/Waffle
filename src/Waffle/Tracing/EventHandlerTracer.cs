namespace Waffle.Tracing
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Events;
    using Waffle.Internal;
    using Waffle.Properties;

    /// <summary>
    /// Tracer for <see cref="ICommandHandler"/>.
    /// </summary>
    internal class EventHandlerTracer : IAsyncEventHandler<IEvent>, IDisposable, IDecorator<IEventHandler>
    {
        private const string DisposeMethodName = "Dispose";
        private const string HandleMethodName = "Handle";

        private readonly IEventHandler innerHandler;
        private readonly HandlerRequest request;
        private readonly ITraceWriter traceWriter;

        public EventHandlerTracer(HandlerRequest request, IEventHandler innerHandler, ITraceWriter traceWriter)
        {
            Contract.Requires(innerHandler != null);
            Contract.Requires(traceWriter != null);

            this.innerHandler = innerHandler;
            this.request = request;
            this.traceWriter = traceWriter;
        }

        public IEventHandler Inner
        {
            get { return this.innerHandler; }
        }

        public ITraceWriter TraceWriter
        {
            get
            {
                return this.traceWriter;
            }
        }

        public EventHandlerContext EventContext
        {
            get { return this.innerHandler.EventContext; }
            set { this.innerHandler.EventContext = value; }
        }

        void IDisposable.Dispose()
        {
            IDisposable disposable = this.innerHandler as IDisposable;
            if (disposable != null)
            {
                this.TraceWriter.TraceBeginEnd(
                    this.request,
                    TraceCategories.HandlersCategory,
                    TraceLevel.Info,
                    this.innerHandler.GetType().Name,
                    DisposeMethodName,
                    beginTrace: null,
                    execute: disposable.Dispose,
                    endTrace: null,
                    errorTrace: null);
            }
        }

        /// <summary>
        /// Handle the command.
        /// </summary>
        /// <param name="event">The <see cref="IEvent"/> to process.</param>
        /// <returns>The result object.</returns>
        public Task HandleAsync(IEvent @event)
        {
            return this.TraceWriter.TraceBeginEndAsync(
                this.innerHandler.EventContext.Request,
                TraceCategories.HandlersCategory,
                TraceLevel.Info,
                this.Inner.GetType().Name,
                HandleMethodName,
                beginTrace: null,
                execute: () => this.innerHandler.EventContext.Descriptor.ExecuteAsync(this.innerHandler.EventContext, default(CancellationToken)),
                endTrace: tr =>
                    {
                        tr.Message = Error.Format(Resources.TraceHandlerExecutedMessage, request.MessageType.FullName);
                    },
                errorTrace: null);
        }
    }
}