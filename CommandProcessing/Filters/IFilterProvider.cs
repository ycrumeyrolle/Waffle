namespace CommandProcessing.Filters
{
    using System.Collections.Generic;

    /// <summary>Provides filter information.</summary>
    public interface IFilterProvider
    {
        /// <summary>Returns an enumeration of filters.</summary>
        /// <returns>An enumeration of filters.</returns>
        /// <param name="configuration">The processor configuration.</param>
        /// <param name="descriptor">The handler descriptor.</param>
        IEnumerable<FilterInfo> GetFilters(ProcessorConfiguration configuration, HandlerDescriptor descriptor);
    }
}