namespace Waffle.Queries
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a context to query. 
    /// </summary>
    public interface IQueryContext : IDisposable
    {
        /// <summary>
        /// Query the context.
        /// </summary>
        /// <typeparam name="T">The type to query.</typeparam>
        /// <returns>A <see cref="IQueryable{T}"/>.</returns>
        IQueryable<T> Query<T>() where T : class;
        /*
        /// <summary>
        /// Find the object.
        /// </summary>
        /// <remarks>Returns <c>null</c> if nothing is found.</remarks>
        /// <typeparam name="T">The type of the object to find.</typeparam>
        /// <param name="keyValues">The values of the primary key for the object to be found.</param>
        /// <returns> The object found, or null.</returns>
        T Find<T>(params object[] keyValues) where T : class;
        */

        /// <summary>
        /// Find the object.
        /// </summary>
        /// <remarks>Returns <c>null</c> if nothing is found.</remarks>
        /// <typeparam name="T">The type of the object to find.</typeparam>
        /// <param name="keyValues">The values of the primary key for the object to be found.</param>
        /// <returns> The object found, or null.</returns>
        Task<T> FindAsync<T>(params object[] keyValues) where T : class;
    }
}