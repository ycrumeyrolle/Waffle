namespace CommandProcessing.Validation
{
    using System;
    using CommandProcessing.Filters;
    using CommandProcessing.Metadata;

    /// <summary>
    /// Validates the body parameter of an action after the parameter has been read by the <see cref="MediaTypeFormatter"/>.
    /// </summary>
    public interface IBodyModelValidator
    {
        /// <summary>
        /// Determines whether the <paramref name="model"/> is valid and adds any validation errors to the <paramref name="actionContext"/>'s <see cref="ModelStateDictionary"/>
        /// </summary>
        /// <param name="model">The model to be validated.</param>
        /// <param name="type">The <see cref="Type"/> to use for validation.</param>
        /// <param name="metadataProvider">The <see cref="ModelMetadataProvider"/> used to provide the model metadata.</param>
        /// <param name="actionContext">The <see cref="HandlerContext"/> within which the model is being validated.</param>
        /// <param name="keyPrefix">The <see cref="string"/> to append to the key for any validation errors.</param>
        /// <returns><c>true</c>if <paramref name="model"/> is valid, <c>false</c> otherwise.</returns>
        bool Validate(object model, Type type, ModelMetadataProvider metadataProvider, HandlerContext actionContext, string keyPrefix);
    }
}
