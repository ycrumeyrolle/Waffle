namespace Waffle.Queries.Data
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Waffle.Internal;
    using Waffle.Queries;

    /// <summary>
    /// Represents a context to query with EntityFramework. 
    /// </summary>
    public class DbQueryContext<TContext> : IQueryContext where TContext : DbContext
    {
        private readonly TContext innerContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbQueryContext{TContext}"/> class.
        /// </summary>
        /// <param name="innerContext">The inner context</param>
        public DbQueryContext(TContext innerContext)
        {
            if (innerContext == null)
            {
                throw Error.ArgumentNull("innerContext");
            }

            this.innerContext = innerContext;
        }

        /// <summary>
        /// Query the context.
        /// </summary>
        /// <typeparam name="T">The type to query.</typeparam>
        /// <returns>A <see cref="System.Linq.IQueryable{T}"/>.</returns>
        public IQueryable<T> Query<T>() where T : class
        {
            return this.innerContext.Set<T>();
        }
        /*
        /// <summary>
        /// Find the object.
        /// </summary>
        /// <remarks>Returns <c>null</c> if nothing is found.</remarks>
        /// <typeparam name="T">The type of the object to find.</typeparam>
        /// <param name="keyValues">The values of the primary key for the object to be found.</param>
        /// <returns>The object found, or null.</returns>
        public T Find<T>(params object[] keyValues) where T : class
        {
            return this.innerContext.Set<T>().Find(keyValues);
        }
        */
        /// <summary>
        /// Find the object.
        /// </summary>
        /// <remarks>Returns <c>null</c> if nothing is found.</remarks>
        /// <typeparam name="T">The type of the object to find.</typeparam>
        /// <param name="keyValues">The values of the primary key for the object to be found.</param>
        /// <returns>The object found, or null.</returns>
        public Task<T> FindAsync<T>(params object[] keyValues) where T : class
        {
            var result = this.innerContext.Set<T>().Find(keyValues);
            return Task.FromResult(result);
        }
        
        /// <summary>
        ///  Calls the protected Dispose method.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the <see cref="DbQueryContext{TContext}"/>. 
        /// The inner context is also disposed. 
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.innerContext.Dispose();
            }
        }
    }
}