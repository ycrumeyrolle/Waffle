namespace CommandProcessing.Unity
{
    using System.Diagnostics.CodeAnalysis;
    using CommandProcessing.Dependencies;
    using Microsoft.Practices.Unity;

    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "False positive. IDisposable is not réimplemented and Dispose method should not be overriden.")]
    public sealed class IocContainer : ScopeContainer, IDependencyResolver
    {
        public IocContainer(IUnityContainer container)
            : base(container)
        {
        }

        public IDependencyScope BeginScope()
        {
            IUnityContainer child = this.Container.CreateChildContainer();
            return new ScopeContainer(child);
        }
    }
}