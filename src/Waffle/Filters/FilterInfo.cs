namespace Waffle.Filters
{
    using Waffle.Internal;

    /// <summary>
    /// Provides information about the available handler filters.
    /// </summary>
    public sealed class FilterInfo
    {
        /// <summary>Initializes a new instance of the <see cref="T:Waffle.Filters.FilterInfo" /> class.</summary>
        /// <param name="instance">The instance of this class.</param>
        /// <param name="scope">The scope of this class.</param>
        public FilterInfo(IFilter instance, FilterScope scope)
        {
            if (instance == null)
            {
                throw Error.ArgumentNull("instance");
            }

            this.Instance = instance;
            this.Scope = scope;
        }

        /// <summary>
        /// Gets an instance of the <see cref="T:Waffle.Filters.FilterInfo"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:Waffle.Filters.FilterInfo"/>.
        /// </returns>
        /// <value>
        /// The instance of the <see cref="T:Waffle.Filters.FilterInfo"/>.
        /// </value>
        public IFilter Instance { get; private set; }

        /// <summary>
        /// Gets the scope <see cref="T:Waffle.Filters.FilterInfo"/>.
        /// </summary>
        /// <returns>
        /// The scope of the FilterInfo.
        /// </returns>
        /// <value>
        /// The <see cref="T:Waffle.Filters.FilterInfo"/>.
        /// </value>
        public FilterScope Scope { get; private set; }
    }
}