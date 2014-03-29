namespace Waffle.Tracing
{
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using Waffle.Events;
    using Waffle.Internal;
    using Waffle.Properties;

    /// <summary>
    /// Tracer for <see cref="IEventHandlerSelector"/>.
    /// </summary>
    internal class EventHandlerSelectorTracer : IEventHandlerSelector, IDecorator<IEventHandlerSelector>
    {
        private const string SelectActionMethodName = "SelectHandler";

        private readonly IEventHandlerSelector innerSelector;
        private readonly ITraceWriter traceWriter;

        public EventHandlerSelectorTracer(IEventHandlerSelector innerSelector, ITraceWriter traceWriter)
        {
            Contract.Requires(innerSelector != null);
            Contract.Requires(traceWriter != null);

            this.innerSelector = innerSelector;
            this.traceWriter = traceWriter;
        }

        public IEventHandlerSelector Inner
        {
            get { return this.innerSelector; }
        }

        EventHandlersDescriptor IEventHandlerSelector.SelectHandlers(EventHandlerRequest request)
        {
            EventHandlersDescriptor eventDescriptor = null;

            this.traceWriter.TraceBeginEnd(
                request,
                TraceCategories.HandlersCategory,
                TraceLevel.Info,
                this.innerSelector.GetType().Name,
                SelectActionMethodName,
                beginTrace: null,
                execute: () => eventDescriptor = this.innerSelector.SelectHandlers(request),
                endTrace: tr =>
                    {
                        tr.Message = Error.Format(
                            Resources.TraceHandlerSelectedMessage,
                            FormattingUtilities.EventHandlerDescriptorsToString(eventDescriptor.EventHandlerDescriptors));
                    },
                errorTrace: null);

            Collection<EventHandlerDescriptor> handlerDescriptors = new Collection<EventHandlerDescriptor>();

            foreach (var handlerDescriptor in eventDescriptor.EventHandlerDescriptors)
            {
                // Intercept returned EventHandlerDescriptor with a tracing version
                if (handlerDescriptor != null && !(handlerDescriptor is EventHandlerDescriptorTracer))
                {
                    handlerDescriptors.Add(new EventHandlerDescriptorTracer(handlerDescriptor, this.traceWriter));
                }
                else
                {
                    handlerDescriptors.Add(handlerDescriptor);
                }
            }

            return new EventHandlersDescriptor(eventDescriptor.EventName, handlerDescriptors);
        }
    }
}