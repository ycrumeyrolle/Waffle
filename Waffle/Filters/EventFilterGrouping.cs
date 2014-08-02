namespace Waffle.Filters
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
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

        private static IEnumerable<T> SelectAvailable<T>(IList<FilterInfo> filters) where T : class
        {
            Contract.Requires(filters != null);
            for (int index = 0; index < filters.Count; index++)
            {
                FilterInfo f = filters[index];
                T instance = f.Instance as T;
                if (instance != null)
                {
                    yield return instance;
                }
            }
        }
    }
}