namespace CommandProcessing.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CommandProcessing.Internal;

    /// <summary>
    /// This <see cref="IFilterProvider"/> implementation retrieves <see cref="FilterInfo">filters</see> associated with an <see cref="HandlerDescriptor"/>
    /// instance.
    /// </summary>   
    public class HandlerFilterProvider : IFilterProvider
    {
        /// <summary>
        /// Returns the collection of filters associated with <paramref name="descriptor"/>.
        /// </summary>
        /// <remarks>
        /// The implementation invokes <see cref="HandlerDescriptor.GetFilters()"/>.
        /// </remarks>
        /// <param name="configuration">The configuration. This value is not used.</param>
        /// <param name="descriptor">The handler descriptor.</param>
        /// <returns>A collection of filters.</returns>
        public IEnumerable<FilterInfo> GetFilters(ProcessorConfiguration configuration, HandlerDescriptor descriptor)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            if (descriptor == null)
            {
                throw Error.ArgumentNull("descriptor");
            }

            IEnumerable<FilterInfo> filters = descriptor.GetFilters().Select(instance => new FilterInfo(instance, FilterScope.Handler));

            return filters;
        }
    }
}