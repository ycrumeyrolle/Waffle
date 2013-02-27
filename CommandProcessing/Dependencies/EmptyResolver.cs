namespace CommandProcessing.Dependencies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class EmptyResolver : IDependencyResolver
    {
        private static readonly IDependencyResolver Singleton = new EmptyResolver();

        private EmptyResolver()
        {
        }

        public static IDependencyResolver Instance
        {
            get
            {
                return EmptyResolver.Singleton;
            }
        }

        /// <inheritdoc/>
        public IDependencyScope BeginScope()
        {
            return this;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <inheritdoc/>
        public object GetService(Type serviceType)
        {
            return null;
        }

        /// <inheritdoc/>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            return Enumerable.Empty<object>();
        }
    }
}