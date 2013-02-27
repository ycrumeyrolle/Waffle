namespace CommandProcessing.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class HandlerFilterProvider : IFilterProvider
    {
        public IEnumerable<FilterInfo> GetFilters(ProcessorConfiguration configuration, HandlerDescriptor descriptor)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            if (descriptor == null)
            {
                throw new ArgumentNullException("descriptor");
            }

            IEnumerable<FilterInfo> filters = descriptor.GetFilters().Select(instance => new FilterInfo(instance, FilterScope.Handler));

            return filters;
        }
    }
}