namespace Waffle.Filters
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Waffle.Internal;
    using Waffle.Metadata;
    using Waffle.Retrying;

    /// <summary>
    /// Represents a descriptor for an handler.
    /// </summary>
    public class HandlerDescriptor
    {
        private readonly ConcurrentDictionary<object, object> properties = new ConcurrentDictionary<object, object>();

        private readonly Lazy<Collection<FilterInfo>> filterPipeline;

        private object[] attributesCached;

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerDescriptor"/> class.
        /// </summary>
        /// <remarks>The default constructor is intended for use by unit testing only.</remarks>
        protected HandlerDescriptor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerDescriptor"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="ProcessorConfiguration"/>.</param>
        /// <param name="messageType">The type of the message.</param>
        /// <param name="handlerType">The type of thr handler.</param>
        protected HandlerDescriptor(ProcessorConfiguration configuration, Type messageType, Type handlerType)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            if (messageType == null)
            {
                throw Error.ArgumentNull("messageType");
            } 
            
            if (handlerType == null)
            {
                throw Error.ArgumentNull("handlerType");
            } 

            this.Configuration = configuration;
            this.HandlerType = handlerType;
            this.MessageType = messageType;
            this.attributesCached = handlerType.GetCustomAttributes(true);
            this.filterPipeline = new Lazy<Collection<FilterInfo>>(this.InitializeFilterPipeline);
            ModelMetadataProvider metadataProvider = this.Configuration.Services.GetModelMetadataProvider();
            ModelMetadata metadata = metadataProvider.GetMetadataForType(null, handlerType);
            this.Name = metadata.GetDisplayName();
            this.Description = metadata.Description;
            this.Lifetime = this.GetHandlerLifetime();
            this.RetryPolicy = this.GetRetryPolicy();
        }

        /// <summary>
        /// Gets or sets the processor configuration.
        /// </summary>
        /// <value>The processor configuration.</value>
        protected ProcessorConfiguration Configuration { get; set; }

        /// <summary>
        /// Gets the attributes of the handler.
        /// </summary>
        /// <value>The attributes of the handler.</value>
        protected ICollection<object> AttributesCached
        {
            get
            {
                return this.attributesCached;
            }
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
        /// Gets the handler type.
        /// </summary>
        /// <value>The handler type.</value>
        public Type HandlerType { get; private set; }

        /// <summary>
        /// Gets the command type.
        /// </summary>
        /// <value>The handler type.</value>
        public Type MessageType { get; private set; }
     
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
        /// Gets the handler lifetime.
        /// </summary>
        /// <value>The handler lifetime.</value>
        public HandlerLifetime Lifetime { get; private set; }
        
        /// <summary>
        /// Gets the <see cref="RetryPolicy"/>.
        /// </summary>
        /// <value>The <see cref="RetryPolicy"/>.</value>
        public RetryPolicy RetryPolicy { get; private set; }

        /// <summary>
        /// Adds attributes to the attributes cache.
        /// </summary>
        /// <param name="attributes">A list of attributes to add.</param>
        protected void AddAttributesToCache(IEnumerable<object> attributes)
        {
            this.attributesCached = this.attributesCached.Concat(attributes).ToArray();
        }

        /// <summary>
        /// Retrieves the filters for the handler descriptor.
        /// </summary>
        /// <returns>The filters for the handler descriptor.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "The method performs a time-consuming operation.")]
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
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "The method performs a time-consuming operation.")]
        public virtual Collection<FilterInfo> GetFilterPipeline()
        {
            return this.filterPipeline.Value;
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
            IFilterProvider[] filterProviders = this.Configuration.Services.GetFilterProviders();

            List<FilterInfo> filters = new List<FilterInfo>();
            for (int i = 0; i < filterProviders.Length; i++)
            {
                IFilterProvider provider = filterProviders[i];
                foreach (FilterInfo filter in provider.GetFilters(this.Configuration, this))
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

        private HandlerLifetime GetHandlerLifetime()
        {
            int length = this.attributesCached.Length;
            for (int i = 0; i < length; i++)
            {
                HandlerLifetimeAttribute handlerLifetimeAttribute = this.attributesCached[i] as HandlerLifetimeAttribute;
                if (handlerLifetimeAttribute != null)
                {
                    return handlerLifetimeAttribute.HandlerLifetime;
                }
            }

            // No attribute were found. Default lifetime is per-request
            return HandlerLifetime.PerRequest;
        }

        private RetryPolicy GetRetryPolicy()
        {
            int length = this.attributesCached.Length;
            for (int i = 0; i < length; i++)
            {
                RetryAttribute retryAttribute = this.attributesCached[i] as RetryAttribute;
                if (retryAttribute != null)
                {
                    return retryAttribute.RetryPolicy;
                }
            }

            // No attribute were found. Default is no retry
            return RetryPolicy.NoRetry;
        }
    }
}