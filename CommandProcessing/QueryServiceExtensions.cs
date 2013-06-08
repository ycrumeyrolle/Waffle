namespace CommandProcessing
{
    using System;
    using CommandProcessing.Internal;
    using CommandProcessing.Queries;

    /// <summary>
    /// Extensions for the <see cref="IQueryService"/>.
    /// </summary>
    public static class QueryServiceExtensions
    {
        /// <summary>
        /// Creates a query context, such as a DbContext.
        /// </summary>
        /// <param name="queryService">The <see cref="IQueryService"/>.</param>
        /// <typeparam name="T">The type of the <see cref="IQueryContext"/>.</typeparam>
        /// <returns>A <see cref="IQueryContext"/> ready to query.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="queryService"/> is <c>null</c>.</exception>
        public static T CreateContext<T>(this IQueryService queryService) where T : IQueryContext
        {
            if (queryService == null)
            {
                throw Error.ArgumentNull("queryService");
            }

            Type contextType = typeof(T);

            return (T)queryService.CreateContext(contextType);
        }

        /// <summary>
        /// Registers a factory to instanciate a new <see cref="IQueryContext"/>.
        /// </summary>
        /// <param name="queryService">The <see cref="IQueryService"/>.</param>
        /// <param name="queryContextFactory"></param>
        /// <typeparam name="T">The type of the <see cref="IQueryContext"/>.</typeparam>
        /// <exception cref="ArgumentNullException"></exception>
        public static void RegisterContextFactory<T>(this IQueryService queryService, Func<IQueryContext> queryContextFactory) where T : IQueryContext
        {
            if (queryService == null)
            {
                throw Error.ArgumentNull("queryService");
            }

            Type type = typeof(T);

            queryService.RegisterContextFactory(type, queryContextFactory);
        }
    }
}