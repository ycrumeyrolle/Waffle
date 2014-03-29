namespace Waffle.Tracing
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using Waffle.Commands;
    using Waffle.Events;
    using Waffle.Filters;

    /// <summary>
    /// Base class and helper for the creation of filter tracers.
    /// </summary>
    internal class FilterTracer : IFilter, IDecorator<IFilter>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterTracer"/> class.
        /// </summary>
        /// <param name="innerFilter">
        /// The inner filter.
        /// </param>
        /// <param name="traceWriter">
        /// The trace writer.
        /// </param>
        public FilterTracer(IFilter innerFilter, ITraceWriter traceWriter)
        {
            Contract.Requires(innerFilter != null);
            Contract.Requires(traceWriter != null);

            this.InnerFilter = innerFilter;
            this.TraceWriter = traceWriter;
        }

        /// <summary>
        /// Gets the inner.
        /// </summary>
        /// <value>
        /// The inner.
        /// </value>
        public IFilter Inner
        {
            get
            {
                return this.InnerFilter;
            }
        }

        /// <summary>
        /// Gets or sets the inner filter.
        /// </summary>
        /// <value>
        /// The inner filter.
        /// </value>
        public IFilter InnerFilter { get; set; }

        /// <summary>
        /// Gets or sets the trace writer.
        /// </summary>
        /// <value>
        /// The trace writer.
        /// </value>
        public ITraceWriter TraceWriter { get; set; }

        /// <summary>
        /// Gets a value indicating whether allow multiple.
        /// </summary>
        /// <value>
        /// The allow multiple.
        /// </value>
        public bool AllowMultiple
        {
            get
            {
                return this.InnerFilter.AllowMultiple;
            }
        }

        /// <summary>
        /// The create filter tracers.
        /// </summary>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <param name="traceWriter">
        /// The trace writer.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public static IEnumerable<IFilter> CreateFilterTracers(IFilter filter, ITraceWriter traceWriter)
        {
            List<IFilter> filters = new List<IFilter>();
            bool addedCommandHandlerAttributeTracer = false;
            bool addedExceptionAttributeTracer = false;
            bool addedEventHandlerAttributeTracer = false;

            CommandHandlerFilterAttribute commandHandlerFilterAttribute = filter as CommandHandlerFilterAttribute;
            if (commandHandlerFilterAttribute != null)
            {
                filters.Add(new CommandHandlerFilterAttributeTracer(commandHandlerFilterAttribute, traceWriter));
                addedCommandHandlerAttributeTracer = true;
            }

            ExceptionFilterAttribute exceptionFilterAttribute = filter as ExceptionFilterAttribute;
            if (exceptionFilterAttribute != null)
            {
                filters.Add(new ExceptionFilterAttributeTracer(exceptionFilterAttribute, traceWriter));
                addedExceptionAttributeTracer = true;
            }

            EventHandlerFilterAttribute eventHandlerFilterAttribute = filter as EventHandlerFilterAttribute;
            if (eventHandlerFilterAttribute != null)
            {
                filters.Add(new EventHandlerFilterAttributeTracer(eventHandlerFilterAttribute, traceWriter));
                addedEventHandlerAttributeTracer = true;
            }

            // Do not add an IHandlerFilter tracer if we already added an HandlerFilterAttribute tracer
            ICommandHandlerFilter commandHandlerFilter = filter as ICommandHandlerFilter;
            if (commandHandlerFilter != null && !addedCommandHandlerAttributeTracer)
            {
                filters.Add(new CommandHandlerFilterTracer(commandHandlerFilter, traceWriter));
            }

            // Do not add an IExceptionFilter tracer if we already added an ExceptoinFilterAttribute tracer
            IExceptionFilter exceptionFilter = filter as IExceptionFilter;
            if (exceptionFilter != null && !addedExceptionAttributeTracer)
            {
                filters.Add(new ExceptionFilterTracer(exceptionFilter, traceWriter));
            }

            // Do not add an IEventHandlerFilter tracer if we already added an EventHandlerFilterAttribute tracer
            IEventHandlerFilter eventHandlerFilter = filter as IEventHandlerFilter;
            if (eventHandlerFilter != null && !addedEventHandlerAttributeTracer)
            {
                filters.Add(new EventHandlerFilterTracer(eventHandlerFilter, traceWriter));
            }

            if (filters.Count == 0)
            {
                filters.Add(new FilterTracer(filter, traceWriter));
            }

            return filters;
        }

        /// <summary>
        /// The create filter tracers.
        /// </summary>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <param name="traceWriter">
        /// The trace writer.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public static IEnumerable<FilterInfo> CreateFilterTracers(FilterInfo filter, ITraceWriter traceWriter)
        {
            Contract.Requires(filter != null);

            IFilter filterInstance = filter.Instance;
            IEnumerable<IFilter> filterTracers = CreateFilterTracers(filterInstance, traceWriter);
            List<FilterInfo> filters = new List<FilterInfo>();
            foreach (IFilter filterTracer in filterTracers)
            {
                filters.Add(new FilterInfo(filterTracer, filter.Scope));
            }

            return filters;
        }

        /// <summary>
        /// Returns a value that indicates whether this filter is a tracer.
        /// </summary>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="filter"/> is a tracer; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsFilterTracer(IFilter filter)
        {
            return filter is FilterTracer || filter is CommandHandlerFilterAttributeTracer || filter is ExceptionFilterAttributeTracer;
        }
    }
}