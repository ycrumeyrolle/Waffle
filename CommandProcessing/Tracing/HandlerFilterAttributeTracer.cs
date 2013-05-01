namespace CommandProcessing.Tracing
{
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using CommandProcessing.Filters;

    /// <summary>
    /// Tracer for <see cref="Filters.HandlerFilterAttribute"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "internal type needs to override, tracer are not sealed")]
    internal class HandlerFilterAttributeTracer : HandlerFilterAttribute, IDecorator<HandlerFilterAttribute>
    {
        private const string ActionExecutedMethodName = "ActionExecuted";

        private const string ActionExecutingMethodName = "ActionExecuting";

        private readonly HandlerFilterAttribute innerFilter;

        private readonly ITraceWriter traceWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerFilterAttributeTracer"/> class.
        /// </summary>
        /// <param name="innerFilter">
        /// The inner filter.
        /// </param>
        /// <param name="traceWriter">
        /// The trace writer.
        /// </param>
        public HandlerFilterAttributeTracer(HandlerFilterAttribute innerFilter, ITraceWriter traceWriter)
        {
            Contract.Assert(innerFilter != null);
            Contract.Assert(traceWriter != null);

            this.innerFilter = innerFilter;
            this.traceWriter = traceWriter;
        }

        /// <summary>
        /// Gets a value indicating whether allow multiple.
        /// </summary>
        /// <value>
        /// The allow multiple.
        /// </value>
        public override bool AllowMultiple
        {
            get
            {
                return this.innerFilter.AllowMultiple;
            }
        }

        /// <summary>
        /// Gets the inner.
        /// </summary>
        /// <value>
        /// The inner.
        /// </value>
        public HandlerFilterAttribute Inner
        {
            get
            {
                return this.innerFilter;
            }
        }

        /// <summary>
        /// Gets the type id.
        /// </summary>
        /// <value>
        /// The type id.
        /// </value>
        public override object TypeId
        {
            get
            {
                return this.innerFilter.TypeId;
            }
        }

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return this.innerFilter.Equals(obj);
        }

        /// <summary>
        /// The get hash code.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return this.innerFilter.GetHashCode();
        }

        /// <summary>
        /// The is default attribute.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool IsDefaultAttribute()
        {
            return this.innerFilter.IsDefaultAttribute();
        }

        /// <summary>
        /// The match.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool Match(object obj)
        {
            return this.innerFilter.Match(obj);
        }

        /// <summary>
        /// The on command executed.
        /// </summary>
        /// <param name="actionExecutedContext">
        /// The handler executed context.
        /// </param>
        public override void OnCommandExecuted(HandlerExecutedContext actionExecutedContext)
        {
            this.traceWriter.TraceBeginEnd(
                actionExecutedContext.Request, 
                TraceCategories.FiltersCategory, 
                TraceLevel.Info, 
                this.innerFilter.GetType().Name, 
                ActionExecutedMethodName, 
                beginTrace: (tr) =>
                {
                    tr.Message = Internal.Error.Format(Resources.TraceActionFilterMessage, FormattingUtilities.ActionDescriptorToString(actionExecutedContext.HandlerContext.Descriptor));
                    tr.Exception = actionExecutedContext.Exception;
                    object response = actionExecutedContext.Result;
                }, 
                execute: () => this.innerFilter.OnCommandExecuted(actionExecutedContext), 
                endTrace: null, 
                errorTrace: null);
        }

        /// <summary>
        /// The on command executing.
        /// </summary>
        /// <param name="handlerContext">
        /// The handler context.
        /// </param>
        public override void OnCommandExecuting(HandlerContext handlerContext)
        {
            this.traceWriter.TraceBeginEnd(
                handlerContext.Request, 
                TraceCategories.FiltersCategory, 
                TraceLevel.Info, 
                this.innerFilter.GetType().Name, 
                ActionExecutingMethodName, 
                beginTrace: (tr) => 
                {
                    tr.Message = Internal.Error.Format(Resources.TraceActionFilterMessage, FormattingUtilities.ActionDescriptorToString(handlerContext.Descriptor)); 
                },
                execute: () => this.innerFilter.OnCommandExecuting(handlerContext), 
                endTrace: null, 
                errorTrace: null);
        }
    }
}