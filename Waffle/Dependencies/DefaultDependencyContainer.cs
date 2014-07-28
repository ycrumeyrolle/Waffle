namespace Waffle.Dependencies
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Reflection;
    using Waffle.Internal;
    using System.Linq;

    public class DefaultDependencyContainer : IDependencyContainer
    {
        private readonly IDictionary<Type, IList<ContainerRegistration>> registry = new Dictionary<Type, IList<ContainerRegistration>>();

        private readonly IDictionary<Type, Func<object>> factoryStore;

        public DefaultDependencyContainer()
            : this(new Dictionary<Type, Func<object>>())
        {
        }

        public DefaultDependencyContainer(IDictionary<Type, Func<object>> factoryStore)
        {
            this.factoryStore = factoryStore; ;
        }

        public ICollection<ContainerRegistration> Registrations
        {
            get
            {
                List<ContainerRegistration> registrations = new List<ContainerRegistration>();
                foreach (var item in this.registry.Values)
                {
                    registrations.AddRange(item);
                }

                return new ReadOnlyCollection<ContainerRegistration>(registrations);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        public object Resolve(Type type)
        {
            Func<object> factory;
            if (type.IsAbstract)
            {
                if (!this.factoryStore.TryGetValue(type, out factory))
                {
                    IList<ContainerRegistration> registrations;
                    if (this.registry.TryGetValue(type, out registrations))
                    {
                        if (registrations.Count > 1)
                        {
                            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unable to instanciate an object of type '{0}'. This type has more than one registration.", type.FullName));
                        }

                        ContainerRegistration registration = registrations[0];
                        factory = this.CreateFactory(registration.MappedToType);
                        this.factoryStore.Add(type, factory);
                    }
                }
            }
            else
            {
                if (!this.factoryStore.TryGetValue(type, out factory))
                {
                    factory = this.CreateFactory(type);
                    this.factoryStore.Add(type, factory);
                }
            }

            return factory();
        }

        public IEnumerable<object> ResolveAll(Type type)
        {
            Func<object> factory;
            if (type.IsAbstract)
            {
                if (!this.factoryStore.TryGetValue(type, out factory))
                {
                    IList<ContainerRegistration> registrations;
                    if (this.registry.TryGetValue(type, out registrations))
                    {
                        foreach (var registration in registrations.Where(r => r.RegisteredType == type))
                        {
                            factory = this.CreateFactory(registration.MappedToType);
                            this.factoryStore.Add(type, factory);
                            yield return factory();
                        }
                    }
                }
            }
            else
            {
                if (!this.factoryStore.TryGetValue(type, out factory))
                {
                    factory = this.CreateFactory(type);
                    this.factoryStore.Add(type, factory);
                    yield return factory();
                }
            }
        }

        public void RegisterInstance(Type type, object instance)
        {
            this.registry.Add(type, null);
            this.factoryStore.Add(type, () => instance);
        }

        public void RegisteType(Type fromType, Type toType)
        {
            if (fromType != null && !fromType.GetTypeInfo().IsGenericType && !toType.GetTypeInfo().IsGenericType)
            {
                Guard.TypeIsAssignable(fromType, toType, "fromType");
            }

            IList<ContainerRegistration> registrations;
            if (!this.registry.TryGetValue(fromType, out registrations))
            {
                registrations = new List<ContainerRegistration>();
                this.registry.Add(fromType, registrations);
            }

            registrations.Add(new ContainerRegistration(fromType, toType));
        }

        private Func<object> CreateFactory(Type type)
        {
            if (this.factoryStore.ContainsKey(type))
            {
                return this.factoryStore[type];
            }

            var ctor = SelectCtor(type);

            if (ctor == null)
            {
                return TypeActivator.Create<object>(type);
            }

            ParameterExpression[] parametersExpression = CreateParametersFactory(ctor);

            var newExpression = Expression.New(ctor, parametersExpression);
            var lambda = Expression.Lambda(newExpression, parametersExpression);

            var compiled = lambda.Compile();

            ParameterInfo[] parameters = ctor.GetParameters();
            Func<object>[] factories = new Func<object>[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                IList<ContainerRegistration> registrations;

                if (!this.registry.TryGetValue(parameter.ParameterType, out registrations))
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unable to instanciate an object of type '{0}' as a parameter. This type is unknown.", parameter.ParameterType.FullName));
                }

                if (registrations.Count == 0)
                {
                    // Is it possible ? Or should it be a simple assertion ?
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unable to instanciate an object of type '{0}' as a parameter. This type has more than one registration.", parameter.ParameterType.FullName));
                }
                if (registrations.Count > 1)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unable to instanciate an object of type '{0}' as a parameter. This type has more than one registration.", parameter.ParameterType.FullName));
                }

                Type parameterType = registrations[0].MappedToType;
                Func<object> parameterFactory = CreateFactory(parameterType);
                factories[i] = parameterFactory;
            }

            Func<object> x = () => compiled.DynamicInvoke(BuildParameters(parameters.Select(p => p.ParameterType).ToArray()));
            return x;
        }

        private object[] BuildParameters(Type[] types)
        {
            object[] parameters = new object[types.Length];
            for (int i = 0; i < types.Length; i++)
            {
                parameters[i] = this.Resolve(types[i]);
            }

            return parameters;
        }

        private ParameterExpression[] CreateParametersFactory(ConstructorInfo ctor)
        {
            ParameterInfo[] parameters = ctor.GetParameters();
            ParameterExpression[] factories = new ParameterExpression[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                Type parameterType = parameter.ParameterType;
                factories[i] = Expression.Parameter(parameterType, parameter.Name);
            }

            return factories;
        }

        private static ConstructorInfo SelectCtor(Type type)
        {
            var ctors = type.GetConstructors();
            ConstructorInfo ctor = null;
            for (int i = 0; i < ctors.Length; i++)
            {
                ConstructorInfo current = ctors[i];

                var currentParameters = current.GetParameters();

                // simple type ==> Unable to instanciate.
                if (currentParameters.Any(p => TypeHelper.IsSimpleType(p.ParameterType)))
                {
                    continue;
                }

                if (ctor == null)
                {
                    ctor = current;
                }
                else
                {
                    var ctorParameters = ctor.GetParameters();
                    if (currentParameters.Length > ctorParameters.Length)
                    {
                        ctor = current;
                    }
                }
            }

            return ctor;
        }
    }

    // Summary:
    //     Class that returns information about the types registered in a container.
    public class ContainerRegistration
    {
        public ContainerRegistration(Type fromType, Type toType)
        {
            this.RegisteredType = fromType;
            this.MappedToType = toType;
        }

        public Type MappedToType { get; private set; }

        public Type RegisteredType { get; private set; }
    }

    public static class Guard
    {
        /// <summary>
        /// Throws <see cref="T:System.ArgumentNullException" /> if the given argument is null.
        /// </summary>
        /// <exception cref="T:System.ArgumentNullException"> if tested value if null.</exception>
        /// <param name="argumentValue">Argument value to test.</param>
        /// <param name="argumentName">Name of the argument being tested.</param>
        public static void ArgumentNotNull(object argumentValue, string argumentName)
        {
            if (argumentValue == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// Throws an exception if the tested string argument is null or the empty string.
        /// </summary>
        /// <exception cref="T:System.ArgumentNullException">Thrown if string value is null.</exception>
        /// <exception cref="T:System.ArgumentException">Thrown if the string is empty</exception>
        /// <param name="argumentValue">Argument value to check.</param>
        /// <param name="argumentName">Name of argument being checked.</param>
        public static void ArgumentNotNullOrEmpty(string argumentValue, string argumentName)
        {
            if (argumentValue == null)
            {
                throw new ArgumentNullException(argumentName);
            }
            if (argumentValue.Length == 0)
            {
                throw new ArgumentException("Must not be empty", argumentName);
            }
        }

        /// <summary>
        /// Verifies that an argument type is assignable from the provided type (meaning
        /// interfaces are implemented, or classes exist in the base class hierarchy).
        /// </summary>
        /// <param name="assignmentTargetType">The argument type that will be assigned to.</param>
        /// <param name="assignmentValueType">The type of the value being assigned.</param>
        /// <param name="argumentName">Argument name.</param>
        public static void TypeIsAssignable(Type assignmentTargetType, Type assignmentValueType, string argumentName)
        {
            if (assignmentTargetType == null)
            {
                throw new ArgumentNullException("assignmentTargetType");
            }
            if (assignmentValueType == null)
            {
                throw new ArgumentNullException("assignmentValueType");
            }
            if (!assignmentTargetType.IsAssignableFrom(assignmentValueType))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Types are not assignables", new object[]
                                                                                                                   {
                                                                                                                       assignmentTargetType,
                                                                                                                       assignmentValueType
                                                                                                                   }), argumentName);
            }
        }

        /// <summary>
        /// Verifies that an argument instance is assignable from the provided type (meaning
        /// interfaces are implemented, or classes exist in the base class hierarchy, or instance can be 
        /// assigned through a runtime wrapper, as is the case for COM Objects).
        /// </summary>
        /// <param name="assignmentTargetType">The argument type that will be assigned to.</param>
        /// <param name="assignmentInstance">The instance that will be assigned.</param>
        /// <param name="argumentName">Argument name.</param>
        public static void InstanceIsAssignable(Type assignmentTargetType, object assignmentInstance, string argumentName)
        {
            if (assignmentTargetType == null)
            {
                throw new ArgumentNullException("assignmentTargetType");
            }
            if (assignmentInstance == null)
            {
                throw new ArgumentNullException("assignmentInstance");
            }
            if (!assignmentTargetType.IsInstanceOfType(assignmentInstance))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Types are not assignables", new object[]
                                                                                                                   {
                                                                                                                       assignmentTargetType,
                                                                                                                       Guard.GetTypeName(assignmentInstance)
                                                                                                                   }), argumentName);
            }
        }

        private static string GetTypeName(object assignmentInstance)
        {
            string result;
            try
            {
                result = assignmentInstance.GetType().FullName;
            }
            catch (Exception)
            {
                result = "Unknow type";
            }
            return result;
        }
    }
}