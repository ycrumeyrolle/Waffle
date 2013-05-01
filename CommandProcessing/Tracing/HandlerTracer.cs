namespace CommandProcessing.Tracing
{
    using System;
    using System.Diagnostics.Contracts;
    using CommandProcessing.Filters;
    using CommandProcessing.Internal;

    /// <summary>
    /// Tracer for <see cref="IHandler"/>.
    /// </summary>
    internal class HandlerTracer : IHandler, IDisposable, IDecorator<IHandler>
    {
        private const string DisposeMethodName = "Dispose";
        private const string HandleMethodName = "Handle";

        private readonly IHandler innerHandler;
        private readonly HandlerRequest request;
        private readonly ITraceWriter traceWriter;

        public HandlerTracer(HandlerRequest request, IHandler innerHandler, ITraceWriter traceWriter)
        {
            Contract.Assert(innerHandler != null);
            Contract.Assert(traceWriter != null);

            this.innerHandler = innerHandler;
            this.request = request;
            this.traceWriter = traceWriter;
        }

        public IHandler Inner
        {
            get { return this.innerHandler; }
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
        /// <param name="command">The <see cref="ICommand"/> to process.</param>
        /// <returns>The result object.</returns>
        public object Handle(ICommand command)
        {
            return this.TraceWriter.TraceBeginEnd<object>(
                this.Inner.Context.Request,
                TraceCategories.HandlersCategory,
                TraceLevel.Info,
                this.Inner.GetType().Name,
                HandleMethodName,
                beginTrace: null,
                execute: () => ((dynamic)this.Inner).Handle(command),
                endTrace: tr =>
                    {
                        tr.Message = Error.Format(Resources.TraceHandlerExecutedMessage, request.CommandType.FullName);
                    },
                errorTrace: null);
        }

        public ICommandProcessor Processor
        {
            get
            {
                return this.innerHandler.Processor;
            }

            set
            {
                this.innerHandler.Processor = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="HandlerContext"/>.
        /// </summary>
        /// <value>The <see cref="HandlerContext"/>.</value>
        public HandlerContext Context
        {
            get
            {
                return this.innerHandler.Context;
            }

            set
            {
                this.innerHandler.Context = value;
            }
        }
        
        public ITraceWriter TraceWriter
        {
            get
            {
                return this.traceWriter;
            }
        }
    }
}