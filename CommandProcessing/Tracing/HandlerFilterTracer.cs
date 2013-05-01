namespace CommandProcessing.Tracing
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using CommandProcessing.Filters;

    /// <summary>
    /// Tracer for <see cref="Filters.IHandlerFilter"/>.
    /// </summary>
    internal class HandlerFilterTracer : FilterTracer, IHandlerFilter, IDecorator<IHandlerFilter>
    {
        private const string ExecuteActionFilterAsyncMethodName = "ExecuteActionFilterAsync";

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerFilterTracer"/> class.
        /// </summary>
        /// <param name="innerFilter">
        /// The inner filter.
        /// </param>
        /// <param name="traceWriter">
        /// The trace writer.
        /// </param>
        public HandlerFilterTracer(IHandlerFilter innerFilter, ITraceWriter traceWriter)
            : base(innerFilter, traceWriter)
        {
        }

        /// <summary>
        /// Gets the inner.
        /// </summary>
        /// <value>
        /// The inner.
        /// </value>
        public new IHandlerFilter Inner
        {
            get
            {
                return this.InnerActionFilter;
            }
        }

        private IHandlerFilter InnerActionFilter
        {
            get
            {
                return this.InnerFilter as IHandlerFilter;
            }
        }

        Task<TResult> IHandlerFilter.ExecuteHandlerFilterAsync<TResult>(HandlerContext handlerContext, CancellationToken cancellationToken, Func<Task<TResult>> continuation)
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