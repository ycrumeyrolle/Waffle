namespace CommandProcessing.Filters
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;
    using CommandProcessing.Dispatcher;
    using CommandProcessing.Internal;
    using CommandProcessing.Services;

    /// <summary>
    /// Provides information about the handler method.
    /// </summary>
    public class HandlerDescriptor
    {
        private readonly object[] attributesCached;

        private ProcessorConfiguration configuration;

        private readonly Lazy<ICollection<FilterInfo>> filterPipeline;

        private readonly ConcurrentDictionary<object, object> properties = new ConcurrentDictionary<object, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerDescriptor"/> class.
        /// </summary>
        /// <remarks>The default constructor is intended for use by unit testing only.</remarks>
        public HandlerDescriptor()
        {
        }

        internal HandlerDescriptor(ProcessorConfiguration configuration, Type handlerType)
        {
            Contract.Requires(configuration != null);
            Contract.Requires(handlerType != null);

            this.configuration = configuration;
            this.HandlerType = handlerType;
            this.filterPipeline = new Lazy<ICollection<FilterInfo>>(this.InitializeFilterPipeline);
            this.attributesCached = handlerType.GetCustomAttributes(true);
            MethodInfo methodInfo = handlerType.GetMethod("Handle", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            this.attributesCached = this.attributesCached.Concat(methodInfo.GetCustomAttributes(true)).ToArray();
            this.HandlerActivator = this.configuration.Services.GetHandlerActivator();
            this.Name = this.configuration.Services.GetHandlerNameResolver().GetHandlerName(this);

            this.Initialize();
        }
        
        /// <summary>
        /// Gets the properties associated with this instance.
        /// </summary>
        /// <value>The properties associated with this instance.</value>
        public ConcurrentDictionary<object, object> Properties
        {
            get
            {
                return this.properties;
            }
        }

        /// <summary>
        /// Gets the <see cref="IHandlerActivator"/> associated with this instance.
        /// </summary>
        /// <value>The <see cref="IHandlerActivator"/> associated with this instance.</value>
        public IHandlerActivator HandlerActivator { get; private set; }

        /// <summary>
        /// Gets the handler name.
        /// </summary>
        /// <value>The handler name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the handler type.
        /// </summary>
        /// <value>The handler type.</value>
        public Type HandlerType { get; private set; }

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
        public Collection<T> GetCustomAttributes<T>() where T : class
        {
            return new Collection<T>(TypeHelper.OfType<T>(this.attributesCached));
        }

        /// <summary>
        /// Retrieves the filters for the given configuration and handler.
        /// </summary>
        /// <returns>The filters for the given configuration and handler.</returns>
        public ICollection<FilterInfo> GetFilterPipeline()
        {
            return this.filterPipeline.Value;
        }

        /// <summary>
        /// Creates a handler instance for the given <see cref="HandlerRequest"/>.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The created handler instance.</returns>
        public Handler CreateHandler(HandlerRequest request)
        {
            return this.HandlerActivator.Create(request, this);
        }

        private static IEnumerable<FilterInfo> RemoveDuplicates(IEnumerable<FilterInfo> filters)
        {
            HashSet<Type> hashSet = new HashSet<Type>();
            foreach (FilterInfo current in filters)
            {
                object instance = current.Instance;
                Type type = instance.GetType();
                if (!hashSet.Contains(type) || HandlerDescriptor.AllowMultiple(instance))
                {
                    yield return current;
                    hashSet.Add(type);
                }
            }
        }

        private static bool AllowMultiple(object filterInstance)
        {
            IFilter filter = filterInstance as IFilter;
            return filter == null || filter.AllowMultiple;
        }

        private ICollection<FilterInfo> InitializeFilterPipeline()
        {
            IEnumerable<IFilterProvider> filterProviders = this.configuration.Services.GetFilterProviders();
            IEnumerable<FilterInfo> source = filterProviders.SelectMany(fp => fp.GetFilters(this.configuration, this)).OrderBy(f => f, FilterInfoComparer.Instance);
            source = HandlerDescriptor.RemoveDuplicates(source.Reverse()).Reverse();
            return source.ToArray();
        }

        // Initialize the Descriptor. This invokes all IHandlerConfiguration attributes
        // on the handler type (and its base types)
        private void Initialize()
        {
            InvokeAttributesOnHandlerType(this, this.HandlerType);
        }

        // Helper to invoke any handler config attributes on this controller type or its base classes.
        private static void InvokeAttributesOnHandlerType(HandlerDescriptor descriptor, Type type)
        {
            Contract.Assert(descriptor != null);

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
                var handlerConfig = attr as IHandlerConfiguration;
                if (handlerConfig != null)
                {
                    var originalConfig = descriptor.configuration;
                    var settings = new HandlerSettings(originalConfig);
                    handlerConfig.Initialize(settings, descriptor);
                    descriptor.configuration = ProcessorConfiguration.ApplyHandlerSettings(settings, originalConfig);
                }
            }
        }
    }
}