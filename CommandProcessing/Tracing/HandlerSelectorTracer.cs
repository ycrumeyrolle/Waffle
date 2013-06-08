namespace CommandProcessing.Tracing
{
    using System.Diagnostics.Contracts;
    using CommandProcessing.Dispatcher;
    using CommandProcessing.Filters;
    using CommandProcessing.Internal;

    /// <summary>
    /// Tracer for <see cref="IHandlerSelector"/>.
    /// </summary>
    internal class HandlerSelectorTracer : IHandlerSelector, IDecorator<IHandlerSelector>
    {
        private const string SelectActionMethodName = "SelectHandler";

        private readonly IHandlerSelector innerSelector;
        private readonly ITraceWriter traceWriter;

        public HandlerSelectorTracer(IHandlerSelector innerSelector, ITraceWriter traceWriter)
        {
            Contract.Assert(innerSelector != null);
            Contract.Assert(traceWriter != null);

            this.innerSelector = innerSelector;
            this.traceWriter = traceWriter;
        }

        public IHandlerSelector Inner
        {
            get { return this.innerSelector; }
        }
        
        HandlerDescriptor IHandlerSelector.SelectHandler(HandlerRequest request)
        {
            HandlerDescriptor handlerDescriptor = null;

            this.traceWriter.TraceBeginEnd<HandlerDescriptor>(
                request,
                TraceCategories.HandlersCategory,
                TraceLevel.Info,
                this.innerSelector.GetType().Name,
                SelectActionMethodName,
                beginTrace: null,
                execute: () => handlerDescriptor = this.innerSelector.SelectHandler(request),
                endTrace: (tr) =>
                    {
                        tr.Message = Error.Format(
                            Resources.TraceHandlerSelectedMessage,
                            FormattingUtilities.HandlerDescriptorToString(handlerDescriptor));
                    },
                errorTrace: null);

            // Intercept returned HttpActionDescriptor with a tracing version
            if (handlerDescriptor != null && !(handlerDescriptor is HandlerDescriptorTracer))   
            {
                return new HandlerDescriptorTracer(handlerDescriptor, this.traceWriter);   
            }

            return handlerDescriptor; 
        }
    }
}