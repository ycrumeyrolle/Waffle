namespace Waffle.Tracing
{
    using System.Diagnostics.Contracts;
    using Waffle.Events;
    using Waffle.Filters;
    using Waffle.Properties;

    /// <summary>
    /// Tracer for <see cref="IEventHandlerActivator"/>.
    /// </summary>
    internal class EventHandlerActivatorTracer : IEventHandlerActivator, IDecorator<IEventHandlerActivator>
    {
        private const string CreateMethodName = "Create";

        private readonly IEventHandlerActivator innerActivator;
        private readonly ITraceWriter traceWriter;

        public EventHandlerActivatorTracer(IEventHandlerActivator innerActivator, ITraceWriter traceWriter)
        {
            Contract.Assert(innerActivator != null);
            Contract.Assert(traceWriter != null);

            this.innerActivator = innerActivator;
            this.traceWriter = traceWriter;
        }

        public IEventHandlerActivator Inner
        {
            get { return this.innerActivator; }
        }

        IEventHandler IEventHandlerActivator.Create(HandlerRequest request, HandlerDescriptor descriptor)
        {
            IEventHandler eventHandler = null;

            this.traceWriter.TraceBeginEnd(
                request,
                TraceCategories.HandlersCategory,
                TraceLevel.Info,
                this.innerActivator.GetType().Name,
                CreateMethodName,
                beginTrace: null,
                execute: () => eventHandler = this.innerActivator.Create(request, descriptor),
                endTrace: tr => tr.Message = eventHandler == null ? Resources.TraceNoneObjectMessage : eventHandler.GetType().FullName,
                errorTrace: null);

            if (eventHandler != null && !(eventHandler is EventHandlerTracer))
            {
                eventHandler = new EventHandlerTracer(request, eventHandler, this.traceWriter);
            }

            return eventHandler;
        }
    }
}