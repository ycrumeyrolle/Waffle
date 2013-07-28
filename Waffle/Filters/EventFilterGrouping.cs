namespace Waffle.Filters
{
    using System.Collections.Generic;
    using System.Linq;
    using Waffle.Events;
    using Waffle.Internal;

    internal class EventFilterGrouping
    {
        private readonly IEventHandlerFilter[] eventHandlerFilters;

        public EventFilterGrouping(IEnumerable<FilterInfo> filters)
        {
            if (filters == null)
            {
                throw Error.ArgumentNull("filters");
            }

            List<FilterInfo> list = filters.AsList();
            this.eventHandlerFilters = SelectAvailable<IEventHandlerFilter>(list).ToArray();
        }

        public IEventHandlerFilter[] EventHandlerFilters
        {
            get
            {
                return this.eventHandlerFilters;
            }
        }
        
        private static IEnumerable<T> SelectAvailable<T>(IEnumerable<FilterInfo> filters)
        {
            return filters.Where(f => (f.Instance is T)).Select(f => (T)f.Instance);
        }
    }
}