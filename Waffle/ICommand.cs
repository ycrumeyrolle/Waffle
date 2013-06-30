namespace Waffle
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Waffle.Validation;

    /// <summary>
    /// Represents a command to transmit to the processor.
    /// The implementations contain all the properties that will be validated and provided to the handlers.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Gets whether the command is valid.
        /// </summary>
        /// <value><c>true</c> id the command is valid ; <c>false</c> otherwise.</value>
        bool IsValid { get; }

        /// <summary>
        /// Gets the validation results collection.
        /// </summary>
        /// <value>The validation results collection.</value>
        ModelStateDictionary ModelState { get; }
    }
}