namespace Waffle
{
    using System.Collections.Generic;
    using System.Reflection;
    using Waffle.Dependencies;

    /// <summary>
    /// Provides an abstraction for managing the assemblies of an application. 
    /// A different implementation can be registered via the <see cref="IDependencyResolver"/>. 
    /// </summary>
    public interface IAssembliesResolver
    {
        /// <summary>
        /// Returns a list of assemblies available for the processor. 
        /// </summary>
        /// <returns>A <see cref="ICollection{Assembly}"/> of assemblies.</returns>
        Assembly[] GetAssemblies();
    }
}