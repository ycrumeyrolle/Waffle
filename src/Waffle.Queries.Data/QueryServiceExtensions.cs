namespace Waffle
{
    using System;
    using System.Data.Entity;
    using Waffle.Internal;
    using Waffle.Queries;
    using Waffle.Queries.Data;

    /// <summary>
    /// Provides methods extensions to the <see cref="ProcessorConfiguration"/>.
    /// </summary>
    public static class DataQueryServiceExtensions
    {
        /// <summary>
        /// Register a <see cref="Func{TContext}"/> as <see cref="DbContext"/> factory.
        /// </summary>
        /// <typeparam name="TContext">The <see cref="DbContext"/> type.</typeparam>
        /// <param name="config">The <see cref="ProcessorConfiguration"/>.</param>
        /// <param name="dbContextFactory">The function factory which creates the <see cref="DbContext"/>.</param>
        public static void RegisterContextFactory<TContext>(this ProcessorConfiguration config, Func<TContext> dbContextFactory) where TContext : DbContext
        {
            if (config == null)
            {
                throw Error.ArgumentNull("config");
            }

            IQueryService queryService = config.Services.GetServiceOrThrow<IQueryService>();

            queryService.RegisterContextFactory<DbQueryContext<TContext>>(() => new DbQueryContext<TContext>(dbContextFactory()));
        }
    }
}
