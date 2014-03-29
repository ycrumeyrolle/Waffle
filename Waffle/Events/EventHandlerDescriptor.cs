namespace Waffle.Events
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Filters;
    using Waffle.Internal;
    using Waffle.Properties;
    using Waffle.Tasks;

    /// <summary>
    /// Provides information about the handler method.
    /// </summary>
    public class EventHandlerDescriptor : HandlerDescriptor
    {
        private EventFilterGrouping filterGrouping;
        private Collection<FilterInfo> filterPipelineForGrouping;
        private readonly Lazy<ActionExecutor> actionExecutor;

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
        /// <param name="handlerType">The type of the handler.</param>
        /// <param name="handleMethod">The <see cref="MethodInfo"/> of the "Handle" method.</param>
        public EventHandlerDescriptor(ProcessorConfiguration configuration, Type eventType, Type handlerType, MethodInfo handleMethod)
            : base(configuration, eventType, handlerType)
        {
            this.AddAttributesToCache(handleMethod.GetCustomAttributes(true));
            this.actionExecutor = new Lazy<ActionExecutor>(() => InitializeActionExecutor(handleMethod, eventType));
            this.handlerActivator = this.Configuration.Services.GetEventHandlerActivator();

            this.Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventHandlerDescriptor"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="ProcessorConfiguration"/>.</param>
        /// <param name="eventType">The type of the message.</param>
        /// <param name="handlerType">The type of the handler.</param>
        public EventHandlerDescriptor(ProcessorConfiguration configuration, Type eventType, Type handlerType)
            : base(configuration, eventType, handlerType)
        {
            var handleMethod = handlerType.GetMethod("Handle", new[] { eventType }) ?? handlerType.GetMethod("HandleAsync", new[] { eventType });
            this.AddAttributesToCache(handleMethod.GetCustomAttributes(true));
            this.actionExecutor = new Lazy<ActionExecutor>(() => InitializeActionExecutor(handleMethod, eventType));
            this.handlerActivator = this.Configuration.Services.GetEventHandlerActivator();

            this.Initialize();
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

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is flowed through the task.")]
        public virtual Task ExecuteAsync(EventHandlerContext context, CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return TaskHelpers.Canceled();
            }

            try
            {
                return this.actionExecutor.Value.Execute(context);
            }
            catch (Exception e)
            {
                return TaskHelpers.FromError(e);
            }
        }

        private static ActionExecutor InitializeActionExecutor(MethodInfo methodInfo, Type eventType)
        {
            Contract.Requires(methodInfo != null);

            if (methodInfo.ContainsGenericParameters)
            {
                throw Error.InvalidOperation(Resources.CommandHandlerDescriptor_CannotCallOpenGenericMethods, methodInfo, methodInfo.ReflectedType.FullName);
            }

            return new ActionExecutor(methodInfo, eventType);
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

        private sealed class ActionExecutor
        {
            private readonly Func<object, IEvent, Task> executor;

            public ActionExecutor(MethodInfo methodInfo, Type eventType)
            {
                Contract.Requires(methodInfo != null);
                Contract.Requires(eventType != null);

                this.executor = GetExecutor(methodInfo, eventType);
            }

            public Task Execute(EventHandlerContext context)
            {
                Contract.Requires(context != null);

                return this.executor(context.Handler, context.Event);
            }
            
            // Method called via reflection.
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Method called via reflection.")]
            private static Task Convert<T>(object taskAsObject)
            {
                Task<T> task = (Task<T>)taskAsObject;
                return task.CastToObject();
            }
            
            private static Func<object, IEvent, Task> GetExecutor(MethodInfo methodInfo, Type eventType)
            {
                Contract.Requires(methodInfo != null);
                Contract.Requires(eventType != null);

                // Parameters to executor
                ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "instance");
                ParameterExpression eventParameter = Expression.Parameter(typeof(IEvent), "event");

                // Call method
                UnaryExpression instanceCast = (!methodInfo.IsStatic) ? Expression.Convert(instanceParameter, methodInfo.ReflectedType) : null;
                Expression eventCast = Expression.Convert(eventParameter, eventType);
                MethodCallExpression methodCall = Expression.Call(instanceCast, methodInfo, eventCast);

                // methodCall is "((MethodInstanceType) instance).Handle(event, context)"
                // Create function
                if (methodCall.Type == typeof(void))
                {
                    // for: public void Action()
                    Expression<Action<object, IEvent>> lambda = Expression.Lambda<Action<object, IEvent>>(methodCall, instanceParameter, eventParameter);
                    Action<object, IEvent> voidExecutor = lambda.Compile();
                    return (instance, @event) =>
                    {
                        voidExecutor(instance, @event);
                        return TaskHelpers.NullResult();
                    };
                }
                else
                {
                    // must coerce methodCall to match Func<object, object[], object> signature
                    UnaryExpression castMethodCall = Expression.Convert(methodCall, typeof(object));
                    Expression<Func<object, IEvent, object>> lambda = Expression.Lambda<Func<object, IEvent, object>>(castMethodCall, instanceParameter, eventParameter);
                    Func<object, IEvent, object> compiled = lambda.Compile();
                    if (methodCall.Type == typeof(Task))
                    {
                        // for: public Task Action()
                        return (instance, @event) =>
                        {
                            Task task = (Task)compiled(instance, @event);
                            ThrowIfWrappedTaskInstance(methodInfo, task.GetType());
                            return task;
                        };
                    }

                    // for: public T Action()
                    return (instance, command) =>
                    {
                        var result = compiled(instance, command);

                        // Throw when the result of a method is Task. Asynchronous methods need to declare that they
                        // return a Task.
                        Task resultAsTask = result as Task;
                        if (resultAsTask != null)
                        {
                            throw Error.InvalidOperation(Resources.ActionExecutor_UnexpectedTaskInstance, methodInfo.Name, methodInfo.DeclaringType.Name);
                        }

                        return Task.FromResult(result);
                    };
                }
            }

            private static void ThrowIfWrappedTaskInstance(MethodInfo method, Type type)
            {
                // Throw if a method declares a return type of Task and returns an instance of Task<Task> or Task<Task<T>>
                // This most likely indicates that the developer forgot to call Unwrap() somewhere.
                Contract.Requires(method != null);
                Contract.Assert(method.ReturnType == typeof(Task));

                // Fast path: check if type is exactly Task first.
                if (type != typeof(Task))
                {
                    Type innerTaskType = TypeHelper.GetTaskInnerTypeOrNull(type);
                    if (innerTaskType != null && typeof(Task).IsAssignableFrom(innerTaskType))
                    {
                        throw Error.InvalidOperation(Resources.ActionExecutor_WrappedTaskInstance, method.Name, method.DeclaringType.Name, type.FullName);
                    }
                }
            }
        }
    }
}