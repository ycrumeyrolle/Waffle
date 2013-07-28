namespace Waffle.Tracing
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Filters;

    /// <summary>
    /// Tracer for <see cref="ICommandHandlerFilter"/>.
    /// </summary>
    internal class CommandHandlerFilterTracer : FilterTracer, ICommandHandlerFilter, IDecorator<ICommandHandlerFilter>
    {
        private const string ExecuteActionFilterAsyncMethodName = "ExecuteActionFilterAsync";

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandlerFilterTracer"/> class.
        /// </summary>
        /// <param name="innerFilter">
        /// The inner filter.
        /// </param>
        /// <param name="traceWriter">
        /// The trace writer.
        /// </param>
        public CommandHandlerFilterTracer(ICommandHandlerFilter innerFilter, ITraceWriter traceWriter)
            : base(innerFilter, traceWriter)
        {
        }

        /// <summary>
        /// Gets the inner.
        /// </summary>
        /// <value>
        /// The inner.
        /// </value>
        public new ICommandHandlerFilter Inner
        {
            get
            {
                return this.InnerActionFilter;
            }
        }

        private ICommandHandlerFilter InnerActionFilter
        {
            get
            {
                return this.InnerFilter as ICommandHandlerFilter;
            }
        }

        Task<TResult> ICommandHandlerFilter.ExecuteHandlerFilterAsync<TResult>(CommandHandlerContext handlerContext, CancellationToken cancellationToken, Func<Task<TResult>> continuation)
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