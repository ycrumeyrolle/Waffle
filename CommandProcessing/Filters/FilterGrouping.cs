namespace CommandProcessing.Filters
{
    using System.Collections.Generic;

    internal class FilterGrouping
    {
        private readonly List<IHandlerFilter> actionFilters = new List<IHandlerFilter>();

        private readonly List<IExceptionFilter> exceptionFilters = new List<IExceptionFilter>();

        public FilterGrouping(IEnumerable<FilterInfo> filters)
        {
            foreach (FilterInfo current in filters)
            {
                IFilter instance = current.Instance;
                FilterGrouping.Categorize(instance, this.actionFilters);
                FilterGrouping.Categorize(instance, this.exceptionFilters);
            }
        }

        public IEnumerable<IHandlerFilter> CommandFilters
        {
            get
            {
                return this.actionFilters;
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