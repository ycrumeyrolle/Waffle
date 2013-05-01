namespace CommandProcessing.Validation
{
    /// <summary>
    /// Represents an interface for the validation of the commands.
    /// </summary>
    public interface ICommandValidator
    {
        /// <summary>
        /// Determines whether the command is valid and adds any validation errors to the command's ValidationResults.
        /// </summary>
        /// <param name="request">The <see cref="HandlerRequest"/> to be validated.</param>
        /// <returns>true if command is valid, false otherwise.</returns>
        bool Validate(HandlerRequest request);
    }
}
