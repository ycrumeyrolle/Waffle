namespace CommandProcessing.Filters
{
    using System.Collections.Generic;
    using CommandProcessing.Internal;

    /// <summary>
    /// This <see cref="IFilterProvider"/> implementation retrieves <see cref="FilterInfo">filters</see> associated with an <see cref="ProcessorConfiguration"/>
    /// instance.
    /// </summary>
    public class ConfigurationFilterProvider : IFilterProvider
    {
        /// <summary>
        /// Returns the collection of filters associated with <paramref name="configuration"/>.
        /// </summary>
        /// <remarks>
        /// The implementation invokes <see cref="ProcessorConfiguration.Filters"/>.
        /// </remarks>
        /// <param name="configuration">The configuration.</param>
        /// <param name="descriptor">The handler descriptor. This value is not used.</param>
        /// <returns>A collection of filters.</returns>
        public IEnumerable<FilterInfo> GetFilters(ProcessorConfiguration configuration, HandlerDescriptor descriptor)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            return configuration.Filters;
        }
    }
}