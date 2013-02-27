namespace CommandProcessing.Descriptions
{
    using System;

    /// <summary>
    /// Describes a <see cref="T:CommandProcessing.ICommand"/>.
    /// </summary>
    public class CommandDescription
    {
        /// <summary>
        /// Gets or sets the name of the command.
        /// </summary>
        /// <value>
        /// The name of the command.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="T:System.Type"/> of the <see cref="T:CommandProcessing.ICommand"/>.
        /// </summary>
        /// <value>
        /// The type of the command.
        /// </value>
        public Type CommandType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="T:System.Type"/> of the <see cref="T:CommandProcessing.ICommandHandler"/>.
        /// </summary>
        /// <value>
        /// The type of the handler.
        /// </value>
        public Type HandlerType { get; set; }
    }
}