namespace Waffle.Tracing
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Events;

    /// <summary>
    /// Tracer for <see cref="IEventHandlerFilter"/>.
    /// </summary>
    internal class EventHandlerFilterTracer : FilterTracer, IEventHandlerFilter, IDecorator<IEventHandlerFilter>
    {
        private const string ExecuteActionFilterAsyncMethodName = "ExecuteHandlerFilterAsync";

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandlerFilterTracer"/> class.
        /// </summary>
        /// <param name="innerFilter">
        /// The inner filter.
        /// </param>
        /// <param name="traceWriter">
        /// The trace writer.
        /// </param>
        public EventHandlerFilterTracer(IEventHandlerFilter innerFilter, ITraceWriter traceWriter)
            : base(innerFilter, traceWriter)
        {
        }

        /// <summary>
        /// Gets the inner.
        /// </summary>
        /// <value>
        /// The inner.
        /// </value>
        public new IEventHandlerFilter Inner
        {
            get
            {
                return this.InnerActionFilter;
            }
        }

        private IEventHandlerFilter InnerActionFilter
        {
            get
            {
                return this.InnerFilter as IEventHandlerFilter;
            }
        }

        Task IEventHandlerFilter.ExecuteHandlerFilterAsync(EventHandlerContext handlerContext, CancellationToken cancellationToken, Func<Task> continuation)
        {
            return this.TraceWriter.TraceBeginEndAsync(
                handlerContext.Request, 
                TraceCategories.FiltersCategory, 
                TraceLevel.Info, 
                this.InnerActionFilter.GetType().Name, 
                ExecuteActionFilterAsyncMethodName, 
                beginTrace: null, 
                execute: () => this.InnerActionFilter.ExecuteHandlerFilterAsync(handlerContext, cancellationToken, continuation), 
                endTrace: null, 
                errorTrace: null);
        }
    }
}