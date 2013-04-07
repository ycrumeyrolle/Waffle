namespace CommandProcessing.Unity
{
    using System.Diagnostics.CodeAnalysis;
    using CommandProcessing.Dependencies;
    using Microsoft.Practices.Unity;

    /// <summary>
    /// Represents a dependency injection container with Unity.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "False positive. IDisposable is not réimplemented and Dispose method should not be overriden.")]
    public sealed class DependencyResolver : DependencyScope, IDependencyResolver
    {    
        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyResolver"/> class. 
        /// </summary>
        public DependencyResolver(IUnityContainer container)
            : base(container)
        {
        }

        public IDependencyScope BeginScope()
        {
            IUnityContainer child = this.Container.CreateChildContainer();
            return new DependencyScope(child);
        }
    }
}