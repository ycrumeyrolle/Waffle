namespace Waffle.Tracing
{
    using System;
    using System.Diagnostics.Contracts;
    using Waffle.Commands;
    using Waffle.Filters;
    using Waffle.Internal;
    using Waffle.Properties;

    /// <summary>
    /// Tracer for <see cref="ICommandHandler"/>.
    /// </summary>
    internal class CommandHandlerTracer : ICommandHandler, IDisposable, IDecorator<ICommandHandler>
    {
        private const string DisposeMethodName = "Dispose";
        private const string HandleMethodName = "Handle";

        private readonly ICommandHandler innerCommandHandler;
        private readonly HandlerRequest request;
        private readonly ITraceWriter traceWriter;

        public CommandHandlerTracer(HandlerRequest request, ICommandHandler innerCommandHandler, ITraceWriter traceWriter)
        {
            Contract.Requires(innerCommandHandler != null);
            Contract.Requires(traceWriter != null);

            this.innerCommandHandler = innerCommandHandler;
            this.request = request;
            this.traceWriter = traceWriter;
        }

        public ICommandHandler Inner
        {
            get { return this.innerCommandHandler; }
        }
        
        public ITraceWriter TraceWriter
        {
            get
            {
                return this.traceWriter;
            }
        }

        public CommandHandlerContext CommandContext
        {
            get { return this.innerCommandHandler.CommandContext; }
            set { this.innerCommandHandler.CommandContext = value; }
        }

        void IDisposable.Dispose()
        {
            IDisposable disposable = this.innerCommandHandler as IDisposable;
            if (disposable != null)
            {
                this.TraceWriter.TraceBeginEnd(
                    this.request,
                    TraceCategories.HandlersCategory,
                    TraceLevel.Info,
                    this.innerCommandHandler.GetType().Name,
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
        public void Handle(ICommand command)
        {
            this.TraceWriter.TraceBeginEnd<object>(
                this.innerCommandHandler.CommandContext.Request,
                TraceCategories.HandlersCategory,
                TraceLevel.Info,
                this.Inner.GetType().Name,
                HandleMethodName,
                beginTrace: null,
                execute: () =>
                {
                    // Critical to allow wrapped handler to have itself in CommandContext
                    this.innerCommandHandler.CommandContext.Handler = ActualHandler(this.innerCommandHandler.CommandContext.Handler);
                    return ExecuteCore(command, this.innerCommandHandler.CommandContext);
                },
                endTrace: tr =>
                    {
                        tr.Message = Error.Format(Resources.TraceHandlerExecutedMessage, request.MessageType.FullName);
                    },
                errorTrace: null);
        }

        public static ICommandHandler ActualHandler(ICommandHandler handler)
        {
            CommandHandlerTracer tracer = handler as CommandHandlerTracer;
            return tracer == null ? handler : tracer.innerCommandHandler;
        }

        private dynamic ExecuteCore(ICommand command, CommandHandlerContext context)
        {
            try
            {
                return ((dynamic)this.Inner).Handle(command, context);
            }
            finally
            {
                IDisposable disposable = this.Inner as IDisposable;

                if (disposable != null)
                {
                    this.request.UnregisterForDispose(disposable, true);
                }
            }
        }
    }
}