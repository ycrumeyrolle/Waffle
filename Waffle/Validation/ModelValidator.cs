namespace Waffle.Validation
{
    using System.Collections.Generic;
    using Waffle.Internal;
    using Waffle.Metadata;

    /// <summary>
    /// Provides a base class for implementing validation logic.
    /// </summary>
    public abstract class ModelValidator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelValidator"/> class.
        /// </summary>
        /// <param name="validatorProviders">The validator providers.</param>
        protected ModelValidator(IEnumerable<ModelValidatorProvider> validatorProviders)
        {
            if (validatorProviders == null)
            {
                throw Error.ArgumentNull("validatorProviders");
            }

            this.ValidatorProviders = validatorProviders;
        }

        /// <summary>
        /// Gets or sets an enumeration of validator providers.
        /// </summary>
        /// <value>
        /// An enumeration of validator providers.
        /// </value>
        protected internal IEnumerable<ModelValidatorProvider> ValidatorProviders { get; private set; }

        /// <summary>
        /// Validates a specified object.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="container">The container.</param>
        /// <returns>A list of validation results.</returns>
        public abstract IEnumerable<ModelValidationResult> Validate(ModelMetadata metadata, object container);
    }
}
