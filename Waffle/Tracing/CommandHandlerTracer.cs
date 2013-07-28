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
            Contract.Assert(innerCommandHandler != null);
            Contract.Assert(traceWriter != null);

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
        /// <param name="context">The <see cref="CommandHandlerContext"/>.</param>
        /// <returns>The result object.</returns>
        public object Handle(ICommand command, CommandHandlerContext context)
        {
            return this.TraceWriter.TraceBeginEnd<object>(
                context.Request,
                TraceCategories.HandlersCategory,
                TraceLevel.Info,
                this.Inner.GetType().Name,
                HandleMethodName,
                beginTrace: null,
                execute: () => ((dynamic)this.Inner).Handle(command, context),
                endTrace: tr =>
                    {
                        tr.Message = Error.Format(Resources.TraceHandlerExecutedMessage, request.MessageType.FullName);
                    },
                errorTrace: null);
        }
    }
}