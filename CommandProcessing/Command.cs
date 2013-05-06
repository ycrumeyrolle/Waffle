namespace CommandProcessing
{
    using System.Linq;
    using CommandProcessing.Caching;
    using CommandProcessing.Validation;

    /// <summary>
    /// Represents a command to transmit to the processor.
    /// The implementations contain all the properties that will be validated and provided to the handlers.
    /// </summary>
    public class Command : ICommand
    {
        private readonly ModelStateDictionary modelStateDictionary = new ModelStateDictionary();

        /// <summary>
        /// Gets whether the command is valid.
        /// </summary>
        /// <value><c>true</c> id the command is valid ; <c>false</c> otherwise.</value>
        [IgnoreCaching]
        public virtual bool IsValid
        {
            get
            {
                return this.modelStateDictionary.Values.All(modelState => modelState.Errors.Count == 0); 
            }
        }

        /// <summary>
        /// Gets the validation results collection.
        /// </summary>
        /// <value>The validation results collection.</value>
        [IgnoreCaching]
        public virtual ModelStateDictionary ModelState
        {
            get
            {
                return this.modelStateDictionary;
            }
        }
    }
}