namespace CommandProcessing.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using CommandProcessing.Filters;

    /// <summary>
    /// Defines the methods that are required for an <see cref="ICommandHandler"/> factory.
    /// </summary>
    public interface IHandlerSelector
    { 
        /// <summary>
        /// Selects a <see cref="HandlerDescriptor"/> for the given <see cref="HandlerRequest"/>.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>An <see cref="HandlerDescriptor"/> instance.</returns>
        HandlerDescriptor SelectHandler(HandlerRequest request);
       
        /// <summary>
        /// Returns a map, keyed by commandType, of all <see cref="HandlerDescriptor"/> that the selector can select. 
        /// This is primarily called by <see cref="CommandProcessing.Descriptions.ICommandExplorer"/> to discover all the possible commands in the system.
        /// </summary>
        /// <returns>A map of all <see cref="HandlerDescriptor"/> that the selector can select, or null if the selector does not have a well-defined mapping of <see cref="HandlerDescriptor"/>.</returns>
        IDictionary<Type, HandlerDescriptor> GetHandlerMapping();
    }
}