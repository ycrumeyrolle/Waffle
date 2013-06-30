namespace Waffle.Filters
{
    using System.Collections.Generic;
    using System.Linq;
    using Waffle.Internal;

    internal class FilterGrouping
    {
        private readonly IHandlerFilter[] handlerFilters;

        private readonly IExceptionFilter[] exceptionFilters;

        public FilterGrouping(IEnumerable<FilterInfo> filters)
        {
            if (filters == null)
            {
                throw Error.ArgumentNull("filters");
            }

            List<FilterInfo> list = filters.AsList();
            this.handlerFilters = SelectAvailable<IHandlerFilter>(list).ToArray();
            this.exceptionFilters = SelectAvailable<IExceptionFilter>(list).ToArray();
        }

        public IHandlerFilter[] HandlerFilters
        {
            get
            {
                return this.handlerFilters;
            }
        }

        public IExceptionFilter[] ExceptionFilters
        {
            get
            {
                return this.exceptionFilters;
            }
        }

        private static IEnumerable<T> SelectAvailable<T>(List<FilterInfo> filters)
        {
            return filters.Where(f => (f.Instance is T)).Select(f => (T)f.Instance);
        }
    }
}