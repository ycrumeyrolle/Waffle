namespace CommandProcessing.Validation
{
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using CommandProcessing.Internal;

    /// <summary>
    /// Represents a class used to recursively validate a command.
    /// </summary>
    /// <remarks>Use internaly the <see cref="System.ComponentModel.DataAnnotations.Validator"/>.</remarks>
    public class SlowCommandValidator : ICommandValidator
    {
        /// <summary>
        /// Determines whether the command is valid and adds any validation errors to the command's ValidationResults.
        /// </summary>
        /// <param name="request">The <see cref="HandlerRequest"/> to be validated.</param>
        /// <returns>true if command is valid, false otherwise.</returns>
        public bool Validate(HandlerRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (request.Command == null)
            {
                throw Error.Argument("request");
            }
            
            ValidationContext context = new ValidationContext(request.Command, null, null);
            Collection<ValidationResult> results = new Collection<ValidationResult>();
            bool valid = Validator.TryValidateObject(request.Command, context, results, true);

            foreach (var result in results)
            {
                request.Command.ModelState.AddModelError(string.Empty, result.ErrorMessage);
            }
            
            return valid;
        }
    }
}
