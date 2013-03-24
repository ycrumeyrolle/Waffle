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
        protected readonly IUnityContainer Container;

        public ScopeContainer(IUnityContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            this.Container = container;
        }

        public object GetService(Type serviceType)
        {
            if (this.Container.IsRegistered(serviceType))
            {
                return this.Container.Resolve(serviceType);
            }

            return null;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            if (this.Container.IsRegistered(serviceType))
            {
                return this.Container.ResolveAll(serviceType);
            }

            return Enumerable.Empty<object>();
        }

        public void Dispose()
        {
            this.Container.Dispose();
        }

        public void RegisterHandlers(ProcessorConfiguration configuration)
        {
            var explorer = configuration.Services.GetCommandExplorer();
            foreach (var description in explorer.Descriptions)
            {
                this.Container.RegisterType(description.HandlerType);
            }
        }
    }
}