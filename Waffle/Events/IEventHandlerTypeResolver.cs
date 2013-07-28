namespace Waffle.Events
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides an abstraction for managing the event handler types of an application. 
    /// A different implementation can be registered via the <see cref="T:Waffle.Dependencies.IDependencyResolver"/>.
    /// </summary>
    public interface IEventHandlerTypeResolver
    {  
        /// <summary>
        /// Returns a list of handlers available for the application.
        /// </summary>
        /// <param name="assembliesResolver">
        /// The <see cref="IAssembliesResolver"/>.
        /// </param>
        /// <returns>
        /// An <see cref="ICollection{Type}"/> of handlers.
        /// </returns>
        ICollection<Type> GetEventHandlerTypes(IAssembliesResolver assembliesResolver);
    }
}