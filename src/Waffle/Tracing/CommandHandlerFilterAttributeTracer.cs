namespace Waffle.Tracing
{
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using Waffle.Commands;
    using Waffle.Filters;
    using Waffle.Internal;
    using Waffle.Properties;

    /// <summary>
    /// Tracer for <see cref="CommandHandlerFilterAttribute"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "internal type needs to override, tracer are not sealed")]
    internal class CommandHandlerFilterAttributeTracer : CommandHandlerFilterAttribute, IDecorator<CommandHandlerFilterAttribute>
    {
        private const string OnCommandExecuteddMethodName = "OnCommandExecuted";

        private const string OnCommandExecutingMethodName = "OnCommandExecuting";

        private readonly CommandHandlerFilterAttribute innerFilter;

        private readonly ITraceWriter traceWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandlerFilterAttributeTracer"/> class.
        /// </summary>
        /// <param name="innerFilter">
        /// The inner filter.
        /// </param>
        /// <param name="traceWriter">
        /// The trace writer.
        /// </param>
        public CommandHandlerFilterAttributeTracer(CommandHandlerFilterAttribute innerFilter, ITraceWriter traceWriter)
        {
            Contract.Requires(innerFilter != null);
            Contract.Requires(traceWriter != null);

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
        public CommandHandlerFilterAttribute Inner
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
        /// <see langword="true"/> if <paramref name="obj"/> equals the type and value of this instance; otherwise, <see langword="false"/>.
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
        /// <see langword="true"/> if this instance is the default attribute for the class; otherwise, <see langword="false"/>.
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
        /// <see langword="true"/> if this instance equals <paramref name="obj"/>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <param name="obj">An <see cref="T:System.Object"/> to compare with this instance of <see cref="T:System.Attribute"/>. </param><filterpriority>2</filterpriority>
        public override bool Match(object obj)
        {
            return this.innerFilter.Match(obj);
        }

        /// <summary>
        /// The on command executed.
        /// </summary>
        /// <param name="commandExecutedContext">
        /// The handler executed context.
        /// </param>
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "response", Justification = "commandExecutedContext.Result must be called.")]
        public override void OnCommandExecuted(CommandHandlerExecutedContext commandExecutedContext)
        {
            if (commandExecutedContext == null)
            {
                throw Error.ArgumentNull("commandExecutedContext");
            }

            this.traceWriter.TraceBeginEnd(
                commandExecutedContext.Request, 
                TraceCategories.FiltersCategory, 
                TraceLevel.Info, 
                this.innerFilter.GetType().Name, 
                OnCommandExecuteddMethodName, 
                beginTrace: tr =>
                {
                    tr.Message = Error.Format(Resources.TraceActionFilterMessage, FormattingUtilities.HandlerDescriptorToString(commandExecutedContext.HandlerContext.Descriptor));
                    tr.Exception = commandExecutedContext.ExceptionInfo.SourceException;
                    object response = commandExecutedContext.Response;
                },
                execute: () => this.innerFilter.OnCommandExecuted(commandExecutedContext), 
                endTrace: null, 
                errorTrace: null);
        }

        /// <summary>
        /// The on command executing.
        /// </summary>
        /// <param name="handlerContext">
        /// The handler context.
        /// </param>
        public override void OnCommandExecuting(CommandHandlerContext handlerContext)
        {
            if (handlerContext == null)
            {
                throw Error.ArgumentNull("handlerContext");
            }

            this.traceWriter.TraceBeginEnd(
                handlerContext.Request, 
                TraceCategories.FiltersCategory, 
                TraceLevel.Info, 
                this.innerFilter.GetType().Name, 
                OnCommandExecutingMethodName, 
                beginTrace: tr => 
                {
                    tr.Message = Error.Format(Resources.TraceActionFilterMessage, FormattingUtilities.HandlerDescriptorToString(handlerContext.Descriptor)); 
                },
                execute: () => this.innerFilter.OnCommandExecuting(handlerContext), 
                endTrace: null, 
                errorTrace: null);
        }
    }
}