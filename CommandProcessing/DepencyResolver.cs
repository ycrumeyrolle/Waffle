////namespace CommandProcessing
////{
////    using System;
////    using System.Collections.Concurrent;
////    using System.Collections.Generic;
////    using System.Linq;
////    using System.Reflection;
////    using System.Globalization;

////    /// <summary>Provides a registration point for dependency resolvers that implement <see cref="T:System.Web.Mvc.IDependencyResolver" /> or the Common Service Locator IServiceLocator interface.</summary>
////    public class DependencyResolver
////    {
////        private sealed class CacheDependencyResolver : IDependencyResolver
////        {
////            private readonly ConcurrentDictionary<Type, object> cache = new ConcurrentDictionary<Type, object>();

////            private readonly ConcurrentDictionary<Type, IEnumerable<object>> cacheMultiple = new ConcurrentDictionary<Type, IEnumerable<object>>();

////            private readonly IDependencyResolver resolver;

////            public CacheDependencyResolver(IDependencyResolver resolver)
////            {
////                this.resolver = resolver;
////            }

////            public object GetService(Type serviceType)
////            {
////                return this.cache.GetOrAdd(serviceType, new Func<Type, object>(this._resolver.GetService));
////            }

////            public IEnumerable<object> GetServices(Type serviceType)
////            {
////                return this.cacheMultiple.GetOrAdd(serviceType, new Func<Type, IEnumerable<object>>(this._resolver.GetServices));
////            }
////        }

////        private class DefaultDependencyResolver : IDependencyResolver
////        {
////            public object GetService(Type serviceType)
////            {
////                if (serviceType.IsInterface || serviceType.IsAbstract)
////                {
////                    return null;
////                }

////                object result;
////                try
////                {
////                    result = Activator.CreateInstance(serviceType);
////                }
////                catch
////                {
////                    result = null;
////                }

////                return result;
////            }

////            public IEnumerable<object> GetServices(Type serviceType)
////            {
////                return Enumerable.Empty<object>();
////            }
////        }

////        private class DelegateBasedDependencyResolver : IDependencyResolver
////        {
////            private Func<Type, object> getService;

////            private Func<Type, IEnumerable<object>> getServices;

////            public DelegateBasedDependencyResolver(Func<Type, object> getService, Func<Type, IEnumerable<object>> getServices)
////            {
////                this.getService = getService;
////                this.getServices = getServices;
////            }

////            public object GetService(Type type)
////            {
////                object result;
////                try
////                {
////                    result = this.getService(type);
////                }
////                catch
////                {
////                    result = null;
////                }

////                return result;
////            }

////            public IEnumerable<object> GetServices(Type type)
////            {
////                return this.getServices(type);
////            }
////        }

////        private static DependencyResolver instance = new DependencyResolver();

////        private IDependencyResolver current;

////        private DependencyResolver.CacheDependencyResolver currentCache;

////        /// <summary>Gets the implementation of the dependency resolver.</summary>
////        /// <returns>The implementation of the dependency resolver.</returns>
////        public static IDependencyResolver Current
////        {
////            get
////            {
////                return DependencyResolver.instance.InnerCurrent;
////            }
////        }

////        internal static IDependencyResolver CurrentCache
////        {
////            get
////            {
////                return DependencyResolver.instance.InnerCurrentCache;
////            }
////        }

////        /// <summary>This API supports the ASP.NET MVC infrastructure and is not intended to be used directly from your code.</summary>
////        /// <returns>The implementation of the dependency resolver.</returns>
////        public IDependencyResolver InnerCurrent
////        {
////            get
////            {
////                return this.current;
////            }
////        }

////        internal IDependencyResolver InnerCurrentCache
////        {
////            get
////            {
////                return this.currentCache;
////            }
////        }

////        /// <summary>Initializes a new instance of the <see cref="T:System.Web.Mvc.DependencyResolver" /> class.</summary>
////        public DependencyResolver()
////        {
////            this.InnerSetResolver(new DependencyResolver.DefaultDependencyResolver());
////        }

////        /// <summary>Provides a registration point for dependency resolvers, using the specified dependency resolver interface.</summary>
////        /// <param name="resolver">The dependency resolver.</param>
////        public static void SetResolver(IDependencyResolver resolver)
////        {
////            DependencyResolver.instance.InnerSetResolver(resolver);
////        }

////        /// <summary>Provides a registration point for dependency resolvers using the provided common service locator when using a service locator interface.</summary>
////        /// <param name="commonServiceLocator">The common service locator.</param>
////        public static void SetResolver(object commonServiceLocator)
////        {
////            DependencyResolver.instance.InnerSetResolver(commonServiceLocator);
////        }

////        /// <summary>Provides a registration point for dependency resolvers using the specified service delegate and specified service collection delegates.</summary>
////        /// <param name="getService">The service delegate.</param>
////        /// <param name="getServices">The services delegates.</param>
////        public static void SetResolver(Func<Type, object> getService, Func<Type, IEnumerable<object>> getServices)
////        {
////            DependencyResolver.instance.InnerSetResolver(getService, getServices);
////        }

////        /// <summary>This API supports the ASP.NET MVC infrastructure and is not intended to be used directly from your code.</summary>
////        /// <param name="resolver">The object that implements the dependency resolver.</param>
////        public void InnerSetResolver(IDependencyResolver resolver)
////        {
////            if (resolver == null)
////            {
////                throw new ArgumentNullException("resolver");
////            }
////            this.current = resolver;
////            this.currentCache = new DependencyResolver.CacheDependencyResolver(this.current);
////        }

////        /// <summary>This API supports the ASP.NET MVC infrastructure and is not intended to be used directly from your code.</summary>
////        /// <param name="commonServiceLocator">The common service locator.</param>
////        public void InnerSetResolver(object commonServiceLocator)
////        {
////            if (commonServiceLocator == null)
////            {
////                throw new ArgumentNullException("commonServiceLocator");
////            }
////            Type type = commonServiceLocator.GetType();
////            MethodInfo method = type.GetMethod("GetInstance", new Type[]
////            {
////                typeof(Type)
////            });
////            MethodInfo method2 = type.GetMethod("GetAllInstances", new Type[]
////            {
////                typeof(Type)
////            });
////            if (method == null || method.ReturnType != typeof(object) || method2 == null || method2.ReturnType != typeof(IEnumerable<object>))
////            {
////                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "The type {0} does not appear to implement Microsoft.Practices.ServiceLocation.IServiceLocator.", new object[]
////                {
////                    type.FullName
////                }), "commonServiceLocator");
////            }            

////            Func<Type, object> getService = (Func<Type, object>)Delegate.CreateDelegate(typeof(Func<Type, object>), commonServiceLocator, method);
////            Func<Type, IEnumerable<object>> getServices = (Func<Type, IEnumerable<object>>)Delegate.CreateDelegate(typeof(Func<Type, IEnumerable<object>>), commonServiceLocator, method2);
////            this.InnerSetResolver(new DependencyResolver.DelegateBasedDependencyResolver(getService, getServices));
////        }

////        /// <summary>This API supports the ASP.NET MVC infrastructure and is not intended to be used directly from your code.</summary>
////        /// <param name="getService">The function that provides the service.</param>
////        /// <param name="getServices">The function that provides the services.</param>
////        public void InnerSetResolver(Func<Type, object> getService, Func<Type, IEnumerable<object>> getServices)
////        {
////            if (getService == null)
////            {
////                throw new ArgumentNullException("getService");
////            }
////            if (getServices == null)
////            {
////                throw new ArgumentNullException("getServices");
////            }

////            this.InnerSetResolver(new DependencyResolver.DelegateBasedDependencyResolver(getService, getServices));
////        }
////    }
////}
