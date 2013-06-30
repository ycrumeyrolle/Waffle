namespace Waffle.Validation.Validators
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Waffle.Internal;
    using Waffle.Metadata;
    using Waffle.Validation;

    /// <summary>
    /// Provides an object adapter that can be validated.
    /// </summary>
    public class ValidatableObjectAdapter : ModelValidator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatableObjectAdapter"/> class.
        /// </summary>
        /// <param name="validatorProviders"></param>
        public ValidatableObjectAdapter(IEnumerable<ModelValidatorProvider> validatorProviders)
            : base(validatorProviders)
        {
        }

        /// <summary>
        /// Validates a specified object.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="container">The container.</param>
        /// <returns>A list of validation results.</returns>
        public override IEnumerable<ModelValidationResult> Validate(ModelMetadata metadata, object container)
        {
            if (metadata == null)
            {
                throw Error.ArgumentNull("metadata");
            }

            // Container is never used here, because IValidatableObject doesn't give you
            // any way to get access to your container.
            object model = metadata.Model;
            if (model == null)
            {
                return Enumerable.Empty<ModelValidationResult>();
            }

            IValidatableObject validatable = model as IValidatableObject;
            if (validatable == null)
            {
                throw Error.InvalidOperation(Resources.ValidatableObjectAdapter_IncompatibleType, model.GetType());
            }

            ValidationContext validationContext = new ValidationContext(validatable, null, null);
            return this.ConvertResults(validatable.Validate(validationContext));
        }

        private IEnumerable<ModelValidationResult> ConvertResults(IEnumerable<ValidationResult> results)
        {
            foreach (ValidationResult result in results)
            {
                if (result != ValidationResult.Success)
                {
                    if (result.MemberNames == null || !result.MemberNames.Any())
                    {
                        yield return new ModelValidationResult { Message = result.ErrorMessage };
                    }
                    else
                    {
                        foreach (string memberName in result.MemberNames)
                        {
                            yield return new ModelValidationResult { Message = result.ErrorMessage, MemberName = memberName };
                        }
                    }
                }
            }
        }
    }
}
