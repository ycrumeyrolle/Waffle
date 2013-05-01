namespace CommandProcessing.Tracing
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using CommandProcessing.Filters;

    /// <summary>
    /// Tracer for <see cref="Filters.ExceptionFilterAttribute"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "internal type needs to override, tracer are not sealed")]
    internal class ExceptionFilterAttributeTracer : ExceptionFilterAttribute, IDecorator<ExceptionFilterAttribute>
    {
        private const string OnExceptionMethodName = "OnException";
        
        private readonly ExceptionFilterAttribute innerFilter;

        private readonly ITraceWriter traceStore;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionFilterAttributeTracer"/> class.
        /// </summary>
        /// <param name="innerFilter">
        /// The inner filter.
        /// </param>
        /// <param name="traceWriter">
        /// The trace writer.
        /// </param>
        public ExceptionFilterAttributeTracer(ExceptionFilterAttribute innerFilter, ITraceWriter traceWriter)
        {
            Contract.Assert(innerFilter != null);
            Contract.Assert(traceWriter != null);

            this.innerFilter = innerFilter;
            this.traceStore = traceWriter;
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
        public ExceptionFilterAttribute Inner
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
        /// The object.
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
        /// The on exception.
        /// </summary>
        /// <param name="handlerExecutedContext">
        /// The handler executed context.
        /// </param>
        public override void OnException(HandlerExecutedContext handlerExecutedContext)
        {
            this.traceStore.TraceBeginEnd(
                handlerExecutedContext.Request, 
                TraceCategories.FiltersCategory, 
                TraceLevel.Info, 
                this.innerFilter.GetType().Name, 
                OnExceptionMethodName, 
                beginTrace: null,
                execute: () => this.innerFilter.OnException(handlerExecutedContext), 
                endTrace: (tr) =>
                {
                    Exception returnedException = handlerExecutedContext.Exception;
                    tr.Level = returnedException == null ? TraceLevel.Info : TraceLevel.Error;
                    tr.Exception = returnedException;
                }, 
                errorTrace: null);
        }
    }
}