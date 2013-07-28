namespace Waffle.Tracing
{
    using System.Diagnostics.Contracts;
    using Waffle.Commands;
    using Waffle.Internal;
    using Waffle.Properties;

    /// <summary>
    /// Tracer for <see cref="ICommandHandlerSelector"/>.
    /// </summary>
    internal class CommandHandlerSelectorTracer : ICommandHandlerSelector, IDecorator<ICommandHandlerSelector>
    {
        private const string SelectActionMethodName = "SelectHandler";

        private readonly ICommandHandlerSelector innerSelector;
        private readonly ITraceWriter traceWriter;

        public CommandHandlerSelectorTracer(ICommandHandlerSelector innerSelector, ITraceWriter traceWriter)
        {
            Contract.Assert(innerSelector != null);
            Contract.Assert(traceWriter != null);

            this.innerSelector = innerSelector;
            this.traceWriter = traceWriter;
        }

        public ICommandHandlerSelector Inner
        {
            get { return this.innerSelector; }
        }

        CommandHandlerDescriptor ICommandHandlerSelector.SelectHandler(CommandHandlerRequest request)
        {
            CommandHandlerDescriptor handlerDescriptor = null;

            this.traceWriter.TraceBeginEnd(
                request,
                TraceCategories.HandlersCategory,
                TraceLevel.Info,
                this.innerSelector.GetType().Name,
                SelectActionMethodName,
                beginTrace: null,
                execute: () => handlerDescriptor = this.innerSelector.SelectHandler(request),
                endTrace: tr =>
                    {
                        tr.Message = Error.Format(
                            Resources.TraceHandlerSelectedMessage,
                            FormattingUtilities.HandlerDescriptorToString(handlerDescriptor));
                    },
                errorTrace: null);

            // Intercept returned HttpActionDescriptor with a tracing version
            if (handlerDescriptor != null && !(handlerDescriptor is CommandHandlerDescriptorTracer))   
            {
                return new CommandHandlerDescriptorTracer(handlerDescriptor, this.traceWriter);   
            }

            return handlerDescriptor; 
        }
    }
}