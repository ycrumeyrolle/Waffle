namespace Waffle.Commands
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Filters;
    using Waffle.Internal;
    using Waffle.Properties;
    using Waffle.Queuing;
    using Waffle.Results;
    using Waffle.Tasks;

    /// <summary>
    /// Provides information about the command handler method.
    /// </summary>
    public class CommandHandlerDescriptor : HandlerDescriptor
    {
        private CommandFilterGrouping filterGrouping;
        private Collection<FilterInfo> filterPipelineForGrouping;

        /// <summary>
        /// Gets the <see cref="ICommandHandlerActivator"/> associated with this instance.
        /// </summary>
        private readonly ICommandHandlerActivator handlerActivator;
        private readonly Lazy<ActionExecutor> actionExecutor;

        private static readonly ResponseMessageResultConverter ResponseMessageResultConverter = new ResponseMessageResultConverter();
        private static readonly VoidResultConverter VoidResultConverter = new VoidResultConverter();
        private IHandlerResultConverter converter;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandlerDescriptor"/> class.
        /// </summary>
        public CommandHandlerDescriptor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandlerDescriptor"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="ProcessorConfiguration"/>.</param>
        /// <param name="commandType">The type of the command.</param>
        /// <param name="handlerType">The type of the handler.</param>
        /// <param name="handleMethod">The <see cref="MethodInfo"/> to be called.</param>
        public CommandHandlerDescriptor(ProcessorConfiguration configuration, Type commandType, Type handlerType, MethodInfo handleMethod)
            : base(configuration, commandType, handlerType)
        {
            this.ReturnType = GetReturnType(handleMethod);
            this.AddAttributesToCache(handleMethod.GetCustomAttributes(true));
            this.actionExecutor = new Lazy<ActionExecutor>(() => InitializeActionExecutor(handleMethod, commandType));
            this.handlerActivator = this.Configuration.Services.GetHandlerActivator();
            this.QueuePolicy = this.GetQueuePolicy();

            this.Initialize();
        }   
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandlerDescriptor"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="ProcessorConfiguration"/>.</param>
        /// <param name="commandType">The type of the command.</param>
        /// <param name="handlerType">The type of the handler.</param>
        public CommandHandlerDescriptor(ProcessorConfiguration configuration, Type commandType, Type handlerType)
            : base(configuration, commandType, handlerType)
        {
            MethodInfo handleMethod = handlerType.GetMethod("Handle", new[] { commandType }) ?? handlerType.GetMethod("HandleAsync", new[] { commandType });
            this.ReturnType = GetReturnType(handleMethod);
            this.AddAttributesToCache(handleMethod.GetCustomAttributes(true));
            this.actionExecutor = new Lazy<ActionExecutor>(() => InitializeActionExecutor(handleMethod, commandType));
            this.handlerActivator = this.Configuration.Services.GetHandlerActivator();
            this.QueuePolicy = this.GetQueuePolicy();

            this.Initialize();
        }

        /// <summary>
        /// Gets the <see cref="QueuePolicy"/>.
        /// </summary>
        /// <value>The <see cref="QueuePolicy"/>.</value
        public QueuePolicy QueuePolicy { get; private set; }

        /// <summary>
        /// Gets the handler result type.
        /// </summary>
        /// <value>The handler result type.</value>
        public Type ReturnType { get; protected set; }

        /// <summary>
        /// Gets the converter for correctly transforming the result of calling
        /// <see cref="ExecuteAsync(CommandHandlerContext, CancellationToken)"/> into an instance of
        /// <see cref="HandlerResponse"/>. 
        /// </summary>
        /// <remarks>
        /// <para>This converter is not used when returning an <see cref="ICommandHandlerResult"/>.</para>
        /// <para>
        /// The behavior of the returned converter should align with the action's declared <see cref="ReturnType"/>.
        /// </para>
        /// </remarks>
        public virtual IHandlerResultConverter ResultConverter
        {
            get
            {
                // This initialization is not thread safe but that's fine since the converters do not have
                // any interesting state. If 2 threads get 2 different instances of the same converter type
                // we don't really care.
                if (this.converter == null)
                {
                    this.converter = GetResultConverter(this.ReturnType);
                }

                return this.converter;
            }
        }

        private QueuePolicy GetQueuePolicy()
        {
            var attributesCached = this.AttributesCached.AsArray();
            int length = attributesCached.Length;
            for (int i = 0; i < length; i++)
            {
                IQueuePolicyProvider queueAttribute = attributesCached[i] as IQueuePolicyProvider;
                if (queueAttribute != null)
                {
                    return queueAttribute.QueuePolicy;
                }
            }

            // No attribute were found. Default queuing policy is used
            return this.Configuration.DefaultQueuePolicy;
        }

        internal static Type GetReturnType(MethodInfo methodInfo)
        {
            Contract.Requires(methodInfo != null);

            Type result = methodInfo.ReturnType;
            if (typeof(Task).IsAssignableFrom(result))
            {
                result = TypeHelper.GetTaskInnerTypeOrNull(methodInfo.ReturnType);
            }

            if (result == typeof(void))
            {
                result = null;
            }

            return result;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is flowed through the task.")]
        public virtual Task<object> ExecuteAsync(CommandHandlerContext context, CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return TaskHelpers.Canceled<object>();
            }

            try
            {
                return this.actionExecutor.Value.ExecuteAsync(context);
            }
            catch (Exception e)
            {
                return TaskHelpers.FromError<object>(e);
            }
        }

        private static ActionExecutor InitializeActionExecutor(MethodInfo methodInfo, Type commandType)
        {
            Contract.Requires(methodInfo != null);

            if (methodInfo.ContainsGenericParameters)
            {
                throw Error.InvalidOperation(Resources.CommandHandlerDescriptor_CannotCallOpenGenericMethods, methodInfo, methodInfo.ReflectedType.FullName);
            }

            return new ActionExecutor(methodInfo, commandType);
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

        internal static IHandlerResultConverter GetResultConverter(Type type)
        {
            if (type != null && type.IsGenericParameter)
            {
                // This can happen if somebody declares an action method as:
                // public T Get<T>() { }
                throw Error.InvalidOperation(Resources.CommandHandlerDescriptor_NoConverterForGenericParamterTypeExists, type);
            }

            if (type == null || type == typeof(void))
            {
                return VoidResultConverter;
            }

            if (typeof(HandlerResponse).IsAssignableFrom(type))
            {
                return ResponseMessageResultConverter;
            }

            if (typeof(ICommandHandlerResult).IsAssignableFrom(type))
            {
                return null;
            }

            Type valueConverterType = typeof(ValueResultConverter<>).MakeGenericType(type);
            return TypeActivator.Create<IHandlerResultConverter>(valueConverterType).Invoke();
        }

        internal class ActionExecutor
        {
            private readonly Func<object, ICommand, Task<object>> executor;
            private static readonly MethodInfo ConvertOfTMethod = typeof(ActionExecutor).GetMethod("Convert", BindingFlags.Static | BindingFlags.NonPublic);

            public ActionExecutor(Func<object, ICommand, Task<object>> executor)
            {
                Contract.Requires(executor != null);

                this.executor = executor;
            }

            public ActionExecutor(MethodInfo methodInfo, Type commandType)
            {
                Contract.Requires(methodInfo != null);
                Contract.Requires(commandType != null);

                this.executor = GetExecutor(methodInfo, commandType);
            }

            public Task<object> ExecuteAsync(CommandHandlerContext context)
            {
                Contract.Requires(context != null);

                return this.executor(context.Handler, context.Command);
            }

            // Method called via reflection.
            private static Task<object> Convert<T>(object taskAsObject)
            {
                Task<T> task = (Task<T>)taskAsObject;
                return task.CastToObject();
            }

            // Do not inline or optimize this method to avoid stack-related reflection demand issues when
            // running from the GAC in medium trust
            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            private static Func<object, Task<object>> CompileGenericTaskConversionDelegate(Type taskValueType)
            {
                Contract.Requires(taskValueType != null);

                return (Func<object, Task<object>>)Delegate.CreateDelegate(typeof(Func<object, Task<object>>), ConvertOfTMethod.MakeGenericMethod(taskValueType));
            }

            private static Func<object, ICommand, Task<object>> GetExecutor(MethodInfo methodInfo, Type commandType)
            {
                Contract.Requires(methodInfo != null);
                Contract.Requires(commandType != null);

                // Parameters to executor
                ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "instance");
                ParameterExpression commandParameter = Expression.Parameter(typeof(ICommand), "command");

                // Call method
                UnaryExpression instanceCast = (!methodInfo.IsStatic) ? Expression.Convert(instanceParameter, methodInfo.ReflectedType) : null;
                Expression commandCast = Expression.Convert(commandParameter, commandType);
                MethodCallExpression methodCall = Expression.Call(instanceCast, methodInfo, commandCast);

                // methodCall is "((MethodInstanceType) instance).Handle(command, context)"
                // Create function
                if (methodCall.Type == typeof(void))
                {
                    // for: public void Action()
                    Expression<Action<object, ICommand>> lambda = Expression.Lambda<Action<object, ICommand>>(methodCall, instanceParameter, commandParameter);
                    Action<object, ICommand> voidExecutor = lambda.Compile();
                    return (instance, command) =>
                    {
                        voidExecutor(instance, command);
                        return TaskHelpers.NullResult();
                    };
                }
                else
                {
                    // must coerce methodCall to match Func<object, object[], object> signature
                    UnaryExpression castMethodCall = Expression.Convert(methodCall, typeof(object));
                    Expression<Func<object, ICommand, object>> lambda = Expression.Lambda<Func<object, ICommand, object>>(castMethodCall, instanceParameter, commandParameter);
                    Func<object, ICommand, object> compiled = lambda.Compile();
                    if (methodCall.Type == typeof(Task))
                    {
                        // for: public Task Action()
                        return (instance, command) =>
                        {
                            Task r = (Task)compiled(instance, command);
                            ThrowIfWrappedTaskInstance(methodInfo, r.GetType());
                            return r.CastToObject();
                        };
                    }

                    if (typeof(Task).IsAssignableFrom(methodCall.Type))
                    {
                        // for: public Task<T> Action()
                        // constructs: return (Task<object>)Convert<T>(((Task<T>)instance).method((T0) param[0], ...))
                        Type taskValueType = TypeHelper.GetTaskInnerTypeOrNull(methodCall.Type);
                        var compiledConversion = CompileGenericTaskConversionDelegate(taskValueType);

                        return (instance, command) =>
                        {
                            object callResult = compiled(instance, command);
                            Task<object> convertedResult = compiledConversion(callResult);
                            return convertedResult;
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
                Contract.Requires(method.ReturnType == typeof(Task));

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
