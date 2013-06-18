namespace CommandProcessing.Filters
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using CommandProcessing.Dispatcher;
    using CommandProcessing.Internal;
    using CommandProcessing.Metadata;

    /// <summary>
    /// Provides information about the handler method.
    /// </summary>
    public class HandlerDescriptor
    {
        private readonly object[] attributesCached;
        private ProcessorConfiguration configuration;
        private FilterGrouping filterGrouping;   
        private Collection<FilterInfo> filterPipelineForGrouping;  

        /// <summary>
        /// Gets the <see cref="IHandlerActivator"/> associated with this instance.
        /// </summary>
        private readonly IHandlerActivator handlerActivator;

        private readonly Lazy<Collection<FilterInfo>> filterPipeline;

        private readonly ConcurrentDictionary<object, object> properties = new ConcurrentDictionary<object, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerDescriptor"/> class.
        /// </summary>
        /// <remarks>The default constructor is intended for use by unit testing only.</remarks>
        public HandlerDescriptor()
        {
        }

        internal HandlerDescriptor(ProcessorConfiguration configuration, Type commandType, Type handlerType)
        {
            Contract.Requires(configuration != null);
            Contract.Requires(handlerType != null);

            this.configuration = configuration;
            this.HandlerType = handlerType;
            this.CommandType = commandType;
            this.filterPipeline = new Lazy<Collection<FilterInfo>>(this.InitializeFilterPipeline);
            this.attributesCached = handlerType.GetCustomAttributes(true);
            var handleMethod = handlerType.GetMethod("Handle", new[] { commandType });
            this.ResultType = handleMethod.ReturnType;
            this.attributesCached = this.attributesCached.Concat(handleMethod.GetCustomAttributes(true)).ToArray();
            this.handlerActivator = this.configuration.Services.GetHandlerActivator();
            ModelMetadataProvider metadataProvider = this.configuration.Services.GetModelMetadataProvider();
            ModelMetadata metadata = metadataProvider.GetMetadataForType(null, handlerType);
            this.Name = metadata.GetDisplayName();
            this.Description = metadata.Description;

            this.Initialize();
        }
        
        /// <summary>
        /// Gets the properties associated with this instance.
        /// </summary>
        /// <value>The properties associated with this instance.</value>
        public virtual ConcurrentDictionary<object, object> Properties
        {
            get
            {
                return this.properties;
            }
        }

        /// <summary>
        /// Gets the handler name.
        /// </summary>
        /// <value>The handler name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the handler description.
        /// </summary>
        /// <value>The handler description.</value>
        public string Description { get; private set; }

        /// <summary>
        /// Gets the handler type.
        /// </summary>
        /// <value>The handler type.</value>
        public Type HandlerType { get; private set; }

        /// <summary>
        /// Gets the command type.
        /// </summary>
        /// <value>The handler type.</value>
        public Type CommandType { get; private set; }

        /// <summary>
        /// Gets the handler result type.
        /// </summary>
        /// <value>The handler result type.</value>
        public Type ResultType { get; private set; }

        /// <summary>
        /// Retrieves the filters for the handler descriptor.
        /// </summary>
        /// <returns>The filters for the handler descriptor.</returns>
        public virtual Collection<IFilter> GetFilters()
        {
            return new Collection<IFilter>(this.GetCustomAttributes<IFilter>().ToList());
        }

        /// <summary>
        /// Returns the custom attributes associated with the handler descriptor.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The custom attributes associated with the handler descriptor.</returns>
        public virtual Collection<T> GetCustomAttributes<T>() where T : class
        {
            return new Collection<T>(TypeHelper.OfType<T>(this.attributesCached));
        }

        /// <summary>
        /// Retrieves the filters for the given configuration and handler.
        /// </summary>
        /// <returns>The filters for the given configuration and handler.</returns>
        public virtual Collection<FilterInfo> GetFilterPipeline()
        {
            return this.filterPipeline.Value;
        }

        /// <summary>
        /// Creates a handler instance for the given <see cref="HandlerRequest"/>.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The created handler instance.</returns>
        public virtual IHandler CreateHandler(HandlerRequest request)
        {
            return this.handlerActivator.Create(request, this);
        }

        internal FilterGrouping GetFilterGrouping()   
        {
            Collection<FilterInfo> currentFilterPipeline = this.GetFilterPipeline();
            if (this.filterGrouping == null || this.filterPipelineForGrouping != currentFilterPipeline)   
            {   
                this.filterGrouping = new FilterGrouping(currentFilterPipeline);
                this.filterPipelineForGrouping = currentFilterPipeline;   
            }

            return this.filterGrouping;   
        }  

        private static void RemoveDuplicates(List<FilterInfo> filters)
        {
            HashSet<Type> hashSet = new HashSet<Type>();
            for (int i = filters.Count - 1; i >= 0; i--)
            {
                FilterInfo current = filters[i];
                object instance = current.Instance;
                Type type = instance.GetType();
                if (!hashSet.Contains(type) || HandlerDescriptor.AllowMultiple(instance))
                {
                    hashSet.Add(type);
                }
                else
                {
                    filters.RemoveAt(i);
                }
            }
        }

        private static bool AllowMultiple(object filterInstance)
        {
            IFilter filter = filterInstance as IFilter;
            return filter == null || filter.AllowMultiple;
        }
        
        private Collection<FilterInfo> InitializeFilterPipeline()
        {
            IFilterProvider[] filterProviders = this.configuration.Services.GetFilterProviders();
            
            List<FilterInfo> filters = new List<FilterInfo>();
            for (int i = 0; i < filterProviders.Length; i++)
            {
                IFilterProvider provider = filterProviders[i];
                foreach (FilterInfo filter in provider.GetFilters(this.configuration, this))
                {
                    filters.Add(filter);
                }
            }

            filters.Sort(FilterInfoComparer.Instance);

            if (filters.Count > 1)
            {
              RemoveDuplicates(filters);
            }

            return new Collection<FilterInfo>(filters);
        }

        // Initialize the Descriptor. This invokes all IHandlerConfiguration attributes
        // on the handler type (and its base types)
        private void Initialize()
        {
            InvokeAttributesOnHandlerType(this, this.HandlerType);
        }

        // Helper to invoke any handler config attributes on this handler type or its base classes.
        private static void InvokeAttributesOnHandlerType(HandlerDescriptor descriptor, Type type)
        {
            Contract.Requires(descriptor != null);

            if (type == null)
            {
                return;
            }

            // Initialize base class before derived classes (same order as ctors).
            InvokeAttributesOnHandlerType(descriptor, type.BaseType);

            // Check for attribute
            object[] attrs = type.GetCustomAttributes(inherit: false);
            for (int i = 0; i < attrs.Length; i++)
            {
                object attr = attrs[i];
                IHandlerConfiguration handlerConfig = attr as IHandlerConfiguration;
                if (handlerConfig != null)
                {
                    ProcessorConfiguration originalConfig = descriptor.configuration;
                    HandlerSettings settings = new HandlerSettings(originalConfig);
                    handlerConfig.Initialize(settings, descriptor);
                    descriptor.configuration = ProcessorConfiguration.ApplyHandlerSettings(settings, originalConfig);
                }
            }
        }
    }
}