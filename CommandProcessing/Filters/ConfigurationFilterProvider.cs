namespace CommandProcessing.Filters
{
    using System;
    using System.Collections.Generic;

    public class ConfigurationFilterProvider : IFilterProvider
    {
        public IEnumerable<FilterInfo> GetFilters(ProcessorConfiguration configuration, HandlerDescriptor descriptor)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            return configuration.Filters;
        }
    }
}