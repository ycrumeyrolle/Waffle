namespace Waffle.Tracing
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using Waffle.Commands;
    using Waffle.Filters;
    using Waffle.Properties;

    /// <summary>
    /// Tracer for <see cref="CommandHandlerDescriptorTracer"/>.
    /// </summary>
    internal class CommandHandlerDescriptorTracer : CommandHandlerDescriptor, IDecorator<CommandHandlerDescriptor>
    {
        private const string CreateHandlerMethodName = "CreateHandler";

        private readonly CommandHandlerDescriptor innerDescriptor;
        private readonly ITraceWriter traceWriter;

        public CommandHandlerDescriptorTracer(CommandHandlerDescriptor innerDescriptor, ITraceWriter traceWriter)
        {
            Contract.Assert(innerDescriptor != null);
            Contract.Assert(traceWriter != null);

            this.innerDescriptor = innerDescriptor;
            this.traceWriter = traceWriter;
        }

        public CommandHandlerDescriptor Inner
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
        public override ICommandHandler CreateHandler(CommandHandlerRequest request)
        {
            ICommandHandler commandHandler = null;

            this.traceWriter.TraceBeginEnd(
                request,
                TraceCategories.HandlersCategory,
                TraceLevel.Info,
                this.innerDescriptor.GetType().Name,
                CreateHandlerMethodName,
                beginTrace: null,
                execute: () =>
                {
                    commandHandler = this.innerDescriptor.CreateHandler(request);
                },
                endTrace: tr => tr.Message = commandHandler == null ? Resources.TraceNoneObjectMessage : this.innerDescriptor.HandlerType.FullName,
                errorTrace: null);

            if (commandHandler != null && !(commandHandler is CommandHandlerTracer))
            {
                return new CommandHandlerTracer(request, commandHandler, this.traceWriter);
            }

            return commandHandler;
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