namespace Waffle.Commands
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using System.Reflection;
    using System.Reflection.Emit;
    using Waffle.Filters;

    /// <summary>
    /// Provides information about the command handler method.
    /// </summary>
    public class CommandHandlerDescriptor : HandlerDescriptor
    {
        private static readonly Type HandlerContextType = typeof(CommandHandlerContext);

        private CommandFilterGrouping filterGrouping;
        private Collection<FilterInfo> filterPipelineForGrouping;

        /// <summary>
        /// Gets the <see cref="ICommandHandlerActivator"/> associated with this instance.
        /// </summary>
        private readonly ICommandHandlerActivator handlerActivator;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandlerDescriptor"/> class.
        /// </summary>
        public CommandHandlerDescriptor()
        {
        }

        public CommandHandlerDescriptor(ProcessorConfiguration configuration, Type commandType, Type handlerType)
            : base(configuration, commandType, handlerType)
        {
            var handleMethod = handlerType.GetMethod("Handle", new[] { commandType, HandlerContextType });
            this.ResultType = handleMethod.ReturnType;
            this.AddAttributesToCache(handleMethod.GetCustomAttributes(true));
            this.handlerActivator = this.Configuration.Services.GetHandlerActivator();
            if (this.ResultType == typeof(void))
            {
                this.HandleVoidMethod = this.CreateDynamicVoidHandleMethod(handleMethod);
            }
            else
            {
                this.HandleMethod = this.CreateDynamicHandleMethod(handleMethod);
            }

            this.Initialize();
        }

        /// <summary>
        /// Gets the Handle method.
        /// </summary>
        /// <value>The Handle method.</value>
        public HandleCommandFunc HandleMethod { get; private set; }

        /// <summary>
        /// Gets the return-less Handle method.
        /// </summary>
        /// <value>The Handle method.</value>
        public VoidCommandAction HandleVoidMethod { get; private set; }

        /// <summary>
        /// Gets the handler result type.
        /// </summary>
        /// <value>The handler result type.</value>
        public Type ResultType { get; private set; }

        private HandleCommandFunc CreateDynamicHandleMethod(MethodInfo handleMethod)
        {
            DynamicMethod dynamicMethod = new DynamicMethod("Handle", typeof(object), new[] { typeof(ICommandHandler), typeof(ICommand), HandlerContextType });
            ILGenerator ilg = dynamicMethod.GetILGenerator();

            // Load the container onto the stack, convert from object => declaring type for the property
            ilg.Emit(OpCodes.Ldarg_0);
            ilg.Emit(OpCodes.Castclass, this.HandlerType);
            ilg.Emit(OpCodes.Ldarg_1);
            ilg.Emit(OpCodes.Castclass, this.MessageType);
            ilg.Emit(OpCodes.Ldarg_2);
            ilg.Emit(OpCodes.Castclass, HandlerContextType);

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

            // Box if the property type is a value type, so it can be returned as an object
            if (handleMethod.ReturnType.IsValueType && handleMethod.ReturnType != typeof(void))
            {
                ilg.Emit(OpCodes.Box, this.ResultType);
            }

            // Return property value
            ilg.Emit(OpCodes.Ret);

            return (HandleCommandFunc)dynamicMethod.CreateDelegate(typeof(HandleCommandFunc));
        }

        private VoidCommandAction CreateDynamicVoidHandleMethod(MethodInfo handleMethod)
        {
            DynamicMethod dynamicMethod = new DynamicMethod("Handle", typeof(void), new[] { typeof(ICommandHandler), typeof(ICommand), HandlerContextType });
            ILGenerator ilg = dynamicMethod.GetILGenerator();

            // Load the container onto the stack, convert from object => declaring type for the property
            ilg.Emit(OpCodes.Ldarg_0);
            ilg.Emit(OpCodes.Castclass, this.HandlerType);
            ilg.Emit(OpCodes.Ldarg_1);
            ilg.Emit(OpCodes.Castclass, this.MessageType);
            ilg.Emit(OpCodes.Ldarg_2);
            ilg.Emit(OpCodes.Castclass, HandlerContextType);

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

            return (VoidCommandAction)dynamicMethod.CreateDelegate(typeof(VoidCommandAction));
        }

        /// <summary>
        /// Creates a handler instance for the given <see cref="HandlerRequest"/>.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The created handler instance.</returns>
        public virtual ICommandHandler CreateHandler(CommandHandlerRequest request)
        {
            return this.handlerActivator.Create(request, this);
        }

        // Initialize the Descriptor. This invokes all IHandlerConfiguration attributes
        // on the handler type (and its base types)
        private void Initialize()
        {
            InvokeAttributesOnHandlerType(this, this.HandlerType);
        }

        internal CommandFilterGrouping GetFilterGrouping()
        {
            Collection<FilterInfo> currentFilterPipeline = this.GetFilterPipeline();
            if (this.filterGrouping == null || this.filterPipelineForGrouping != currentFilterPipeline)
            {
                this.filterGrouping = new CommandFilterGrouping(currentFilterPipeline);
                this.filterPipelineForGrouping = currentFilterPipeline;
            }

            return this.filterGrouping;
        }

        // Helper to invoke any handler config attributes on this handler type or its base classes.
        private static void InvokeAttributesOnHandlerType(CommandHandlerDescriptor descriptor, Type type)
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
                    handlerConfig.Initialize(settings, descriptor);
                    descriptor.Configuration = ProcessorConfiguration.ApplyHandlerSettings(settings, originalConfig);
                }
            }
        }
    }
}
