namespace Waffle.Tracing
{
    using System.Diagnostics.Contracts;
    using Waffle.Dispatcher;
    using Waffle.Filters;

    /// <summary>
    /// Tracer for <see cref="IHandlerActivator"/>.
    /// </summary>
    internal class HandlerActivatorTracer : IHandlerActivator, IDecorator<IHandlerActivator>
    {
        private const string CreateMethodName = "Create";

        private readonly IHandlerActivator innerActivator;
        private readonly ITraceWriter traceWriter;

        public HandlerActivatorTracer(IHandlerActivator innerActivator, ITraceWriter traceWriter)
        {
            Contract.Assert(innerActivator != null);
            Contract.Assert(traceWriter != null);

            this.innerActivator = innerActivator;
            this.traceWriter = traceWriter;
        }

        public IHandlerActivator Inner
        {
            get { return this.innerActivator; }
        }

        IHandler IHandlerActivator.Create(HandlerRequest request, HandlerDescriptor descriptor)
        {
            IHandler handler = null;

            this.traceWriter.TraceBeginEnd(
                request,
                TraceCategories.HandlersCategory,
                TraceLevel.Info,
                this.innerActivator.GetType().Name,
                CreateMethodName,
                beginTrace: null,
                execute: () => handler = this.innerActivator.Create(request, descriptor),
                endTrace: (tr) => tr.Message = handler == null ? Resources.TraceNoneObjectMessage : handler.GetType().FullName,
                errorTrace: null);

            if (handler != null && !(handler is HandlerTracer))
            {
                handler = new HandlerTracer(request, handler, this.traceWriter);
            }

            return handler;
        }
    }
}