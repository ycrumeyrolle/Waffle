namespace CommandProcessing.Filters
{
    /// <summary>Définit des membres qui spécifient l'ordre des filtres et si plusieurs filtres sont autorisés.</summary>
    public interface IFilter
    {
        /// <summary>
        /// Gets a value indicating whether multiple filters are allowed.
        /// </summary>
        /// <returns>
        /// true if multiple filters are allowed; otherwise, false.
        /// </returns>
        /// <value>
        /// The allow multiple.
        /// </value>
        bool AllowMultiple { get; }
    }
}