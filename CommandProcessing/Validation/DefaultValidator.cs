namespace CommandProcessing.Validation
{
    using System.ComponentModel.DataAnnotations;
    using CommandProcessing.Internal;

    /// <summary>
    /// Represents a class used to recursively validate a command.
    /// </summary>
    /// <remarks>Use internaly the <see cref="System.ComponentModel.DataAnnotations.Validator"/>.</remarks>
    public class DefaultCommandValidator : ICommandValidator
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
            bool valid = Validator.TryValidateObject(request.Command, context, request.Command.ValidationResults, true);
            
            return valid;
        }
    }
}
