namespace Waffle.Events
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Defines the methods that are required to provide <see cref="EventHandlerDescriptor"/>s.
    /// </summary>
    public interface IEventHandlerDescriptorProvider
    {
        /// <summary>
        /// Returns a map, keyed by eventType, of all <see cref="EventHandlerDescriptor"/> that the provider can select. 
        /// </summary>
        /// <returns>A map of all <see cref="EventHandlerDescriptor"/> that the provider can select, or null if the provider does not have a well-defined mapping of <see cref="EventHandlerDescriptor"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "The method performs a time-consuming operation.")]
        IDictionary<Type, EventHandlersDescriptor> GetHandlerMapping();
    }
}