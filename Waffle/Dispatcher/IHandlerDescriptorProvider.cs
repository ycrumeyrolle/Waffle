namespace Waffle.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using Waffle.Filters;

    /// <summary>
    /// Defines the methods that are required to provide <see cref="HandlerDescriptor"/>s.
    /// </summary>
    public interface IHandlerDescriptorProvider
    {
        /// <summary>
        /// Returns a map, keyed by commandType, of all <see cref="HandlerDescriptor"/> that the provider can select. 
        /// </summary>
        /// <returns>A map of all <see cref="HandlerDescriptor"/> that the provider can select, or null if the provider does not have a well-defined mapping of <see cref="HandlerDescriptor"/>.</returns>
        IDictionary<Type, HandlerDescriptor> GetHandlerMapping();
    }
}