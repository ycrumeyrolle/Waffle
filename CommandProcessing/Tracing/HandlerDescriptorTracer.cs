namespace CommandProcessing.Tracing
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using CommandProcessing.Filters;

    /// <summary>
    /// Tracer for <see cref="HandlerDescriptorTracer"/>.
    /// </summary>
    internal class HandlerDescriptorTracer : HandlerDescriptor, IDecorator<HandlerDescriptor>
    {
        private const string CreateHandlerMethodName = "CreateHandler";

        private readonly HandlerDescriptor innerDescriptor;
        private readonly ITraceWriter traceWriter;

        public HandlerDescriptorTracer(HandlerDescriptor innerDescriptor, ITraceWriter traceWriter)
        {
            Contract.Assert(innerDescriptor != null);
            Contract.Assert(traceWriter != null);

            this.innerDescriptor = innerDescriptor;
            this.traceWriter = traceWriter;
        }

        public HandlerDescriptor Inner
        {
            get { return this.innerDescriptor; }
        }

        public override ConcurrentDictionary<object, object> Properties
        {
            get
            {
                return this.innerDescriptor.Properties;
            }
        }

        public override Collection<T> GetCustomAttributes<T>()
        {
            return this.innerDescriptor.GetCustomAttributes<T>();
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "This object is returned back to the caller")]
        public override IHandler CreateHandler(HandlerRequest request)
        {
            IHandler handler = null;

            this.traceWriter.TraceBeginEnd(
                request,
                TraceCategories.HandlersCategory,
                TraceLevel.Info,
                this.innerDescriptor.GetType().Name,
                CreateHandlerMethodName,
                beginTrace: null,
                execute: () =>
                {
                    handler = this.innerDescriptor.CreateHandler(request);
                },
                endTrace: tr => tr.Message = handler == null ? Resources.TraceNoneObjectMessage : this.innerDescriptor.HandlerType.FullName,
                errorTrace: null);

            if (handler != null && !(handler is HandlerTracer))
            {
                return new HandlerTracer(request, handler, this.traceWriter);
            }

            return handler;
        }

        public override Collection<IFilter> GetFilters()
        {
            List<IFilter> filters = new List<IFilter>(this.innerDescriptor.GetFilters());
            List<IFilter> returnFilters = new List<IFilter>(filters.Count);
            for (int i = 0; i < filters.Count; i++)
            {
                if (FilterTracer.IsFilterTracer(filters[i]))
                {
                    returnFilters.Add(filters[i]);
                }
                else
                {
                    IEnumerable<IFilter> filterTracers = FilterTracer.CreateFilterTracers(filters[i], this.traceWriter);
                    foreach (IFilter filterTracer in filterTracers)
                    {
                        returnFilters.Add(filterTracer);
                    }
                }
            }

            return new Collection<IFilter>(returnFilters);
        }

        public override Collection<FilterInfo> GetFilterPipeline()
        {
            List<FilterInfo> filters = new List<FilterInfo>(this.innerDescriptor.GetFilterPipeline());
            List<FilterInfo> returnFilters = new List<FilterInfo>(filters.Count);
            for (int i = 0; i < filters.Count; i++)
            {
                // If this filter has been wrapped already, use as is
                if (FilterTracer.IsFilterTracer(filters[i].Instance))
                {
                    returnFilters.Add(filters[i]);
                }
                else
                {
                    IEnumerable<FilterInfo> filterTracers = FilterTracer.CreateFilterTracers(filters[i], this.traceWriter);
                    foreach (FilterInfo filterTracer in filterTracers)
                    {
                        returnFilters.Add(filterTracer);
                    }
                }
            }

            return new Collection<FilterInfo>(returnFilters);
        }
    }
}