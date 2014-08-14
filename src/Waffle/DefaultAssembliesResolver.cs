namespace Waffle
{
    using System;
    using System.Collections.ObjectModel;
    using System.Reflection;

    /// <summary>
    /// Provides an implementation of <see cref="IAssembliesResolver"/> with no external dependencies.
    /// </summary>
    public class DefaultAssembliesResolver : IAssembliesResolver
    { 
        /// <summary>
        /// Returns a list of assemblies available for the application.
        /// </summary>
        /// <returns>A <see cref="Collection{T}"/> of assemblies.</returns>
        public Assembly[] GetAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }
    }
}