namespace CommandProcessing.Unity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CommandProcessing.Dependencies;
    using CommandProcessing.Services;
    using Microsoft.Practices.Unity;

    public class ScopeContainer : IDependencyScope
    {
        private readonly IUnityContainer container;

        public ScopeContainer(IUnityContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            this.container = container;
        }

        protected IUnityContainer Container
        {
            get
            {
                return this.container;
            }
        }

        public object GetService(Type serviceType)
        {
            if (this.container.IsRegistered(serviceType))
            {
                return this.container.Resolve(serviceType);
            }

            return null;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            if (this.container.IsRegistered(serviceType))
            {
                return this.container.ResolveAll(serviceType);
            }

            return Enumerable.Empty<object>();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.container != null)
                {
                    this.container.Dispose();
                }
            }
        }

        public void RegisterHandlers(ProcessorConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            var explorer = configuration.Services.GetCommandExplorer();
            foreach (var description in explorer.Descriptions)
            {
                this.container.RegisterType(description.HandlerType);
            }
        }
    }
}