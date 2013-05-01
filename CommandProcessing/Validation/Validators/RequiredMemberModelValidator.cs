//namespace CommandProcessing.Validation.Validators
//{
//    using System.Collections.Generic;
//    using CommandProcessing.Metadata;
//    using CommandProcessing.Validation;

//    /// <summary>
//    /// <see cref="ModelValidator"/> for required members.
//    /// </summary>
//    public class RequiredMemberModelValidator : ModelValidator
//    {
//        public RequiredMemberModelValidator(IEnumerable<ModelValidatorProvider> validatorProviders)
//            : base(validatorProviders)
//        {
//        }

//        public override bool IsRequired
//        {
//            get { return true; }
//        }

//        public override IEnumerable<ModelValidationResult> Validate(ModelMetadata metadata, object container)
//        {
//            return new ModelValidationResult[0];
//        }
//    }
//}
