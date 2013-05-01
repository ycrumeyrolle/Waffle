namespace CommandProcessing.Tracing
{
    using System.Threading;
    using System.Threading.Tasks;
    using CommandProcessing.Filters;

    /// <summary>
    /// Tracer for <see cref="IExceptionFilter"/>.
    /// </summary>
    internal class ExceptionFilterTracer : FilterTracer, IExceptionFilter, IDecorator<IExceptionFilter>
    {
        private const string ExecuteExceptionFilterAsyncMethodName = "ExecuteExceptionFilterAsync";
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionFilterTracer"/> class.
        /// </summary>
        /// <param name="innerFilter">
        /// The inner filter.
        /// </param>
        /// <param name="traceWriter">
        /// The trace writer.
        /// </param>
        public ExceptionFilterTracer(IExceptionFilter innerFilter, ITraceWriter traceWriter)
            : base(innerFilter, traceWriter)
        {
        }
        
        /// <summary>
        /// Gets the inner.
        /// </summary>
        /// <value>
        /// The inner.
        /// </value>
        public new IExceptionFilter Inner
        {
            get
            {
                return this.InnerExceptionFilter;
            }
        }

        /// <summary>
        /// Gets the inner exception filter.
        /// </summary>
        /// <value>
        /// The inner exception filter.
        /// </value>
        public IExceptionFilter InnerExceptionFilter
        {
            get
            {
                return this.InnerFilter as IExceptionFilter;
            }
        }
        
        /// <summary>
        /// The execute exception filter async.
        /// </summary>
        /// <param name="actionExecutedContext">
        /// The handler executed context.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public Task ExecuteExceptionFilterAsync(HandlerExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            return this.TraceWriter.TraceBeginEndAsync(
                actionExecutedContext.Request, 
                TraceCategories.FiltersCategory, 
                TraceLevel.Info, 
                this.InnerExceptionFilter.GetType().Name, 
                ExecuteExceptionFilterAsyncMethodName, 
                beginTrace: tr => tr.Exception = actionExecutedContext.Exception, 
                execute: () => this.InnerExceptionFilter.ExecuteExceptionFilterAsync(actionExecutedContext, cancellationToken), 
                endTrace: tr => tr.Exception = actionExecutedContext.Exception, 
                errorTrace: null);
        }
    }
}