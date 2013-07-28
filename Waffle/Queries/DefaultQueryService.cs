namespace Waffle.Queries
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq.Expressions;
    using Waffle.Internal;
    using Waffle.Properties;

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
                throw Error.ArgumentNull("contextType");
            }

            Func<IQueryContext> factory;
            if (!this.queryableAdapterFactories.TryGetValue(contextType, out factory))
            {
                throw Error.InvalidOperation(Resources.QueryService_NoQueryContext, contextType.Name);
            }

            IQueryContext queryableContext = factory();
            if (queryableContext == null)
            {
                throw Error.InvalidOperation(Resources.QueryService_NoQueryContextFactory, contextType.Name);
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
        public void RegisterContextFactory<T>(Type contextType, Func<T> queryContextFactory) where T : IQueryContext
        {
            if (queryContextFactory == null)
            {
                throw Error.ArgumentNull("queryContextFactory");
            }

            if (contextType == null)
            {
                throw Error.ArgumentNull("contextType");
            }

            Func<IQueryContext> func = CastFunc<T, IQueryContext>(queryContextFactory);
            this.queryableAdapterFactories.TryAdd(contextType, func);
        }

        private static Func<TTo> CastFunc<TFrom, TTo>(Func<TFrom> func)
        {
            Expression<Func<TFrom>> expression = () => func();
            Expression converted = Expression.Convert(expression.Body, typeof(TTo));

            return Expression.Lambda<Func<TTo>>(converted, expression.Parameters).Compile();
        }
    }
}