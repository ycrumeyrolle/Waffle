namespace CommandProcessing.Filters
{
    using System.Collections.Generic;
    using CommandProcessing.Internal;

    internal class FilterGrouping
    {
        private readonly List<IHandlerFilter> handlerFilters = new List<IHandlerFilter>();

        private readonly List<IExceptionFilter> exceptionFilters = new List<IExceptionFilter>();

        public FilterGrouping(IEnumerable<FilterInfo> filters)
        {
            if (filters == null)
            {
                throw Error.ArgumentNull("filters");
            }

            var list = filters.AsList();
            for (int i = 0; i < list.Count; i++)
            {
                FilterInfo current = list[i];
                IFilter instance = current.Instance;
                FilterGrouping.Categorize(instance, this.handlerFilters);
                FilterGrouping.Categorize(instance, this.exceptionFilters);
            }
        }

        public IEnumerable<IHandlerFilter> HandlerFilters
        {
            get
            {
                return this.handlerFilters;
            }
        }

        public IEnumerable<IExceptionFilter> ExceptionFilters
        {
            get
            {
                return this.exceptionFilters;
            }
        }

        private static void Categorize<T>(IFilter filter, List<T> list) where T : class
        {
            T t = filter as T;
            if (t != null)
            {
                list.Add(t);
            }
        }
    }
}