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
    using CommandProcessing.Services;

    public class HandlerDescriptor
    {
        private readonly object[] attributesCached;

        private ProcessorConfiguration configuration;

        private readonly Lazy<Collection<FilterInfo>> filterPipeline;

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
            this.configuration = configuration;
            this.HandlerType = handlerType;
            this.filterPipeline = new Lazy<Collection<FilterInfo>>(this.InitializeFilterPipeline);
            this.attributesCached = handlerType.GetCustomAttributes(true);
            MethodInfo methodInfo = handlerType.GetMethod("Handle", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            this.attributesCached = this.attributesCached.Concat(methodInfo.GetCustomAttributes(true)).ToArray();
            this.HandlerActivator = this.configuration.Services.GetHandlerActivator();
            this.Name = this.configuration.Services.GetHandlerNameResolver().GetHandlerName(this);

            this.Initialize();
        }
        
        public ConcurrentDictionary<object, object> Properties
        {
            get
            {
                return this.properties;
            }
        }

        public IHandlerActivator HandlerActivator { get; private set; }

        public string Name { get; private set; }

        public Type HandlerType { get; private set; }

        public virtual Collection<IFilter> GetFilters()
        {
            return new Collection<IFilter>(this.GetCustomAttributes<IFilter>().ToList());
        }

        public Collection<T> GetCustomAttributes<T>() where T : class
        {
            return new Collection<T>(TypeHelper.OfType<T>(this.attributesCached));
        }

        public Collection<FilterInfo> GetFilterPipeline()
        {
            return this.filterPipeline.Value;
        }

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

        private Collection<FilterInfo> InitializeFilterPipeline()
        {
            IEnumerable<IFilterProvider> filterProviders = this.configuration.Services.GetFilterProviders();
            IEnumerable<FilterInfo> source = filterProviders.SelectMany(fp => fp.GetFilters(this.configuration, this)).OrderBy(f => f, FilterInfoComparer.Instance);
            source = HandlerDescriptor.RemoveDuplicates(source.Reverse()).Reverse();
            return new Collection<FilterInfo>(source.ToList());
        }

        // Initialize the Descriptor. This invokes all IHandlerConfiguration attributes
        // on the handler type (and its base types)
        private void Initialize()
        {
            InvokeAttributesOnHandlerType(this, this.HandlerType);
        }

        // Helper to invoke any Controller config attributes on this controller type or its base classes.
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
            foreach (object attr in attrs)
            {
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