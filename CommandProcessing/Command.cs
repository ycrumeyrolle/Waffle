namespace CommandProcessing
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using CommandProcessing.Caching;

    /// <summary>
    /// Represents a command to transmit to the processor.
    /// The implementations contain all the properties that will be validated and provided to the handlers.
    /// </summary>
    public class Command : ICommand
    {
        private readonly ICollection<ValidationResult> validationResults = new List<ValidationResult>();

        /// <summary>
        /// Gets whether the command is valid.
        /// </summary>
        /// <value><c>true</c> id the command is valid ; <c>false</c> otherwise.</value>
        [IgnoreCaching]
        public virtual bool IsValid
        {
            get
            {
                return this.validationResults.Count == 0;
            }
        }

        /// <summary>
        /// Gets the validation results collection.
        /// </summary>
        /// <value>The validation results collection.</value>
        [IgnoreCaching]
        public virtual ICollection<ValidationResult> ValidationResults
        {
            get
            {
                return this.validationResults;
            }
        }
    }
}