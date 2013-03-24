namespace CommandProcessing.Unity
{
    using CommandProcessing.Dependencies;
    using Microsoft.Practices.Unity;

    public class IocContainer : ScopeContainer, IDependencyResolver
    {
        public IocContainer(IUnityContainer container)
            : base(container)
        {
        }

        public IDependencyScope BeginScope()
        {
            var child = this.Container.CreateChildContainer();
            return new ScopeContainer(child);
        }
    }
}