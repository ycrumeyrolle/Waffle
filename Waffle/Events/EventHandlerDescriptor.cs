namespace Waffle.Events
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using System.Reflection;
    using System.Reflection.Emit;
    using Waffle.Commands;
    using Waffle.Filters;

    /// <summary>
    /// Provides information about the handler method.
    /// </summary>
    public class EventHandlerDescriptor : HandlerDescriptor
    {
        private EventFilterGrouping filterGrouping;
        private Collection<FilterInfo> filterPipelineForGrouping;

        /// <summary>
        /// Gets the <see cref="IEventHandlerActivator"/> associated with this instance.
        /// </summary>
        private readonly IEventHandlerActivator handlerActivator;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandlerDescriptor"/> class.
        /// </summary>
        /// <remarks>The default constructor is intended for use by unit testing only.</remarks>
        public EventHandlerDescriptor()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="EventHandlerDescriptor"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="ProcessorConfiguration"/>.</param>
        /// <param name="eventType">The type of the message.</param>
        /// <param name="handlerType">The type of thr handler.</param>
        public EventHandlerDescriptor(ProcessorConfiguration configuration, Type eventType, Type handlerType)
            : base(configuration, eventType, handlerType)
        {
            var handleMethod = handlerType.GetMethod("Handle", new[] { eventType, typeof(EventHandlerContext) });
            this.AddAttributesToCache(handleMethod.GetCustomAttributes(true));
            this.HandleMethod = this.CreateDynamicHandleMethod(handleMethod);

            this.handlerActivator = this.Configuration.Services.GetEventHandlerActivator();
            
            this.Initialize();
        }

        /// <summary>
        /// Gets the Handle method.
        /// </summary>
        /// <value>The Handle method.</value>
        public HandleEventAction HandleMethod { get; private set; }

        private HandleEventAction CreateDynamicHandleMethod(MethodInfo handleMethod)
        {
            DynamicMethod dynamicMethod = new DynamicMethod("Handle", typeof(void), new[] { typeof(IEventHandler), typeof(IEvent), typeof(EventHandlerContext) });
            ILGenerator ilg = dynamicMethod.GetILGenerator();

            // Load the container onto the stack, convert from object => declaring type for the property
            ilg.Emit(OpCodes.Ldarg_0);
            ilg.Emit(OpCodes.Castclass, this.HandlerType);
            ilg.Emit(OpCodes.Ldarg_1);
            ilg.Emit(OpCodes.Castclass, this.MessageType);
            ilg.Emit(OpCodes.Ldarg_2);
            ilg.Emit(OpCodes.Castclass, typeof(EventHandlerContext));

            // if declaring type is value type, we use Call : structs don't have inheritance
            // if get method is sealed or isn't virtual, we use Call : it can't be overridden
            if (this.HandlerType.IsValueType || !handleMethod.IsVirtual || handleMethod.IsFinal)
            {
                ilg.Emit(OpCodes.Call, handleMethod);
            }
            else
            {
                ilg.Emit(OpCodes.Callvirt, handleMethod);
            }
            
            // Return property value
            ilg.Emit(OpCodes.Ret);

            return (HandleEventAction)dynamicMethod.CreateDelegate(typeof(HandleEventAction));
        }
        
        /// <summary>
        /// Creates a handler instance for the given <see cref="HandlerRequest"/>.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The created handler instance.</returns>
        public virtual IEventHandler CreateHandler(EventHandlerRequest request)
        {
            return this.handlerActivator.Create(request, this);
        }

        // Initialize the Descriptor. This invokes all IHandlerConfiguration attributes
        // on the handler type (and its base types)
        private void Initialize()
        {
            InvokeAttributesOnHandlerType(this, this.HandlerType);
        }

        internal EventFilterGrouping GetFilterGrouping()
        {
            Collection<FilterInfo> currentFilterPipeline = this.GetFilterPipeline();
            if (this.filterGrouping == null || this.filterPipelineForGrouping != currentFilterPipeline)
            {
                this.filterGrouping = new EventFilterGrouping(currentFilterPipeline);
                this.filterPipelineForGrouping = currentFilterPipeline;
            }

            return this.filterGrouping;
        }

        // Helper to invoke any handler config attributes on this handler type or its base classes.
        private static void InvokeAttributesOnHandlerType(EventHandlerDescriptor descriptor, Type type)
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
                    ProcessorConfiguration originalConfig = descriptor.Configuration;
                    CommandHandlerSettings settings = new CommandHandlerSettings(originalConfig);
                    //// handlerConfig.Initialize(settings, descriptor);
                    descriptor.Configuration = ProcessorConfiguration.ApplyHandlerSettings(settings, originalConfig);
                }
            }
        }
    }
}