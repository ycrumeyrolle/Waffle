namespace Waffle.Filters
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using Waffle.Internal;

    internal class CommandFilterGrouping
    {
        private readonly ICommandHandlerFilter[] commandHandlerFilters;

        private readonly IExceptionFilter[] exceptionFilters;

        public CommandFilterGrouping(IEnumerable<FilterInfo> filters)
        {
            if (filters == null)
            {
                throw Error.ArgumentNull("filters");
            }

            List<FilterInfo> list = filters.AsList();
            this.commandHandlerFilters = SelectAvailable<ICommandHandlerFilter>(list).ToArray();
            this.exceptionFilters = SelectAvailable<IExceptionFilter>(list).ToArray();
        }

        public ICommandHandlerFilter[] CommandHandlerFilters
        {
            get
            {
                return this.commandHandlerFilters;
            }
        }

        public IExceptionFilter[] ExceptionFilters
        {
            get
            {
                return this.exceptionFilters;
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