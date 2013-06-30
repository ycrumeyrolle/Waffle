namespace Waffle.Tracing
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using Waffle.Filters;
    using Waffle.Internal;

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
        /// When implemented in a derived class, gets a unique identifier for this <see cref="T:System.Attribute"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Object"/> that is a unique identifier for the attribute.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override object TypeId
        {
            get
            {
                return this.innerFilter.TypeId;
            }
        }

        /// <summary>
        /// Returns a value that indicates whether this instance is equal to a specified object.
        /// </summary>
        /// <returns>
        /// <c>true</c> if <paramref name="obj"/> equals the type and value of this instance; otherwise, <c>false</c>.
        /// </returns>
        /// <param name="obj">An <see cref="T:System.Object"/> to compare with this instance or null. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            return this.innerFilter.Equals(obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer hash code.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return this.innerFilter.GetHashCode();
        }

        /// <summary>
        /// When overridden in a derived class, indicates whether the value of this instance is the default value for the derived class.
        /// </summary>
        /// <returns>
        /// <c>true</c>  if this instance is the default attribute for the class; otherwise, <c>false</c>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override bool IsDefaultAttribute()
        {
            return this.innerFilter.IsDefaultAttribute();
        }

        /// <summary>
        /// When overridden in a derived class, returns a value that indicates whether this instance equals a specified object.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance equals <paramref name="obj"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <param name="obj">An <see cref="T:System.Object"/> to compare with this instance of <see cref="T:System.Attribute"/>. </param><filterpriority>2</filterpriority>
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
            if (handlerExecutedContext == null)
            {
                throw Error.ArgumentNull("handlerExecutedContext");
            }

            this.traceStore.TraceBeginEnd(
                handlerExecutedContext.Request, 
                TraceCategories.FiltersCategory, 
                TraceLevel.Info, 
                this.innerFilter.GetType().Name, 
                OnExceptionMethodName, 
                beginTrace: null,
                execute: () => this.innerFilter.OnException(handlerExecutedContext), 
                endTrace: tr =>
                    {
                        Exception returnedException = handlerExecutedContext.Exception;
                        tr.Level = returnedException == null ? TraceLevel.Info : TraceLevel.Error;
                        tr.Exception = returnedException;
                    }, 
                errorTrace: null);
        }
    }
}