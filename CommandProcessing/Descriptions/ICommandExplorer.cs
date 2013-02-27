namespace CommandProcessing.Descriptions
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines the interface for getting a collection of <see cref="CommandDescription"/>.
    /// </summary>
    public interface ICommandExplorer
    {   
        /// <summary>
        /// Gets the command descriptions.
        /// </summary>
        /// <value>
        /// The descriptions.
        /// </value>
        ICollection<CommandDescription> Descriptions { get; }
    }
}