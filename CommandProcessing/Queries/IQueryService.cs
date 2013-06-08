namespace CommandProcessing.Queries
{
    using System;
    
    /// <summary>
    /// Provides a way to query the system.
    /// </summary>
    public interface IQueryService
    {
        /// <summary>
        /// Creates a <see cref="IQueryContext"/>.
        /// </summary>
        /// <param name="contextType">The <see cref="IQueryContext"/> type.</param>
        /// <returns>The <see cref="IQueryContext"/>.</returns>
        IQueryContext CreateContext(Type contextType);

        /// <summary>
        /// Registers a factory to instanciate a new <see cref="IQueryContext"/>.
        /// </summary>
        /// <param name="contextType">The <see cref="IQueryContext"/> type.</param>
        /// <param name="queryContextFactory">The factory method.</param>
        void RegisterContextFactory(Type contextType, Func<IQueryContext> queryContextFactory);
    }
}