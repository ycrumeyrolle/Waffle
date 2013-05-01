//namespace CommandProcessing.Validation.Validators
//{
//    using System.Collections.Generic;
//    using CommandProcessing.Internal;
//    using CommandProcessing.Metadata;
//    using CommandProcessing.Validation;

//    /// <summary>
//    /// A <see cref="ModelValidator"/> to represent an error. This validator will always throw an exception regardless of the actual model value.
//    /// </summary>
//    public class ErrorModelValidator : ModelValidator
//    {
//        private readonly string errorMessage;

//        public ErrorModelValidator(IEnumerable<ModelValidatorProvider> validatorProviders, string errorMessage)
//            : base(validatorProviders)
//        {
//            if (errorMessage == null)
//            {
//                throw Error.ArgumentNull("errorMessage");
//            }

//            this.errorMessage = errorMessage;
//        }

//        public override IEnumerable<ModelValidationResult> Validate(ModelMetadata metadata, object container)
//        {
//            throw Error.InvalidOperation(this.errorMessage);
//        }
//    }
//}
