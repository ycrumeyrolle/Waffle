namespace CommandProcessing.Filters
{
    using System;

    /// <summary>
    /// Provides information about the available action filters.
    /// </summary>
    public sealed class FilterInfo
    {
        /// <summary>Initializes a new instance of the <see cref="T:CommandProcessing.Filters.FilterInfo" /> class.</summary>
        /// <param name="instance">The instance of this class.</param>
        /// <param name="scope">The scope of this class.</param>
        public FilterInfo(IFilter instance, FilterScope scope)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            this.Instance = instance;
            this.Scope = scope;
        }

        /// <summary>
        /// Gets an instance of the <see cref="T:CommandProcessing.Filters.FilterInfo"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:CommandProcessing.Filters.FilterInfo"/>.
        /// </returns>
        /// <value>
        /// The instance of the <see cref="T:CommandProcessing.Filters.FilterInfo"/>.
        /// </value>
        public IFilter Instance { get; private set; }

        /// <summary>
        /// Gets the scope <see cref="T:CommandProcessing.Filters.FilterInfo"/>.
        /// </summary>
        /// <returns>
        /// The scope of the FilterInfo.
        /// </returns>
        /// <value>
        /// The <see cref="T:CommandProcessing.Filters.FilterInfo"/>.
        /// </value>
        public FilterScope Scope { get; private set; }
    }
}