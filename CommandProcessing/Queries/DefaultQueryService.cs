namespace CommandProcessing.Queries
{
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// Provides a default implementation of the <see cref="IQueryService"/> interface.
    /// </summary>
    public class DefaultQueryService : IQueryService
    {
        private readonly ConcurrentDictionary<Type, Func<IQueryContext>> queryableAdapterFactories = new ConcurrentDictionary<Type, Func<IQueryContext>>();

        /// <summary>
        /// Creates a <see cref="IQueryContext"/>.
        /// </summary>
        /// <param name="contextType">The <see cref="IQueryContext"/> type.</param>
        /// <returns>The <see cref="IQueryContext"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="contextType"/> is null.</exception>
        /// <exception cref="InvalidOperationException">No <see cref="IQueryContext"/> factory was found.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="IQueryContext"/> factory return null.</exception>
        public IQueryContext CreateContext(Type contextType)
        {
            if (contextType == null)
            {
                throw new ArgumentNullException("contextType");
            }

            Func<IQueryContext> factory;
            if (!this.queryableAdapterFactories.TryGetValue(contextType, out factory))
            {
                throw new InvalidOperationException(string.Format("No IQueryContext factory found for '{0}'.", contextType.Name));
            }

            IQueryContext queryableContext = factory();
            if (queryableContext == null)
            {
                throw new InvalidOperationException(string.Format("IQueryContext factory for '{0}' return null.", contextType.Name));
            }

            return queryableContext;
        }

        /// <summary>
        /// Registers a factory to instanciate a new <see cref="IQueryContext"/>.
        /// </summary>
        /// <param name="contextType">The <see cref="IQueryContext"/> type.</param>
        /// <param name="queryContextFactory">The factory method.</param>
        /// <exception cref="ArgumentNullException"><paramref name="contextType"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="queryContextFactory"/> is null.</exception>
        public void RegisterContextFactory(Type contextType, Func<IQueryContext> queryContextFactory)
        {
            if (queryContextFactory == null)
            {
                throw new ArgumentNullException("queryContextFactory");
            }

            if (contextType == null)
            {
                throw new ArgumentNullException("contextType");
            }

            this.queryableAdapterFactories.TryAdd(contextType, queryContextFactory);
        }
    }
}