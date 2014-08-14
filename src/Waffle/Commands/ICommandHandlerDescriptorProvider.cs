namespace Waffle.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Defines the methods that are required to provide <see cref="CommandHandlerDescriptor"/>s.
    /// </summary>
    public interface ICommandHandlerDescriptorProvider
    {
        /// <summary>
        /// Returns a map, keyed by commandType, of all <see cref="CommandHandlerDescriptor"/> that the provider can select. 
        /// </summary>
        /// <returns>A map of all <see cref="CommandHandlerDescriptor"/> that the provider can select, or null if the provider does not have a well-defined mapping of <see cref="CommandHandlerDescriptor"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "The method performs a time-consuming operation.")]
        IDictionary<Type, CommandHandlerDescriptor> GetHandlerMapping();
    }
}