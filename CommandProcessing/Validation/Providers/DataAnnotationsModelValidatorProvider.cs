namespace CommandProcessing.Validation.Providers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    using CommandProcessing.Internal;
    using CommandProcessing.Metadata;
    using CommandProcessing.Validation;
    using CommandProcessing.Validation.Validators;

    /// <summary>
    /// Represents the method that creates a <see cref="DataAnnotationsModelValidatorProvider"/> instance.
    /// </summary>
    /// <param name="validatorProviders">An enumeration of validator providers.</param>
    /// <param name="attribute">The attribute.</param>
    /// <returns>The <see cref="ModelValidator"/></returns>
    public delegate ModelValidator DataAnnotationsModelValidationFactory(IEnumerable<ModelValidatorProvider> validatorProviders, ValidationAttribute attribute);

    /// <summary>
    /// Provides a factory for validators that are based on <see cref="IValidatableObject"/>.
    /// </summary>
    /// <param name="validatorProviders">An enumeration of validator providers.</param>
    /// <returns>The <see cref="ModelValidator"/></returns>
    public delegate ModelValidator DataAnnotationsValidatableObjectAdapterFactory(IEnumerable<ModelValidatorProvider> validatorProviders);

    /// <summary>
    /// An implementation of <see cref="ModelValidatorProvider"/> which providers validators
    /// for attributes which derive from <see cref="ValidationAttribute"/>. It also provides
    /// a validator for types which implement <see cref="IValidatableObject"/>.
    /// </summary>
    public class DataAnnotationsModelValidatorProvider : AssociatedValidatorProvider
    {
        // Factories for validation attributes
        private readonly DataAnnotationsModelValidationFactory defaultAttributeFactory = (validationProviders, attribute) => new DataAnnotationsModelValidator(validationProviders, attribute);

        private readonly Dictionary<Type, DataAnnotationsModelValidationFactory> attributeFactories = new Dictionary<Type, DataAnnotationsModelValidationFactory>();

        // Factories for IValidatableObject models
        private readonly DataAnnotationsValidatableObjectAdapterFactory defaultValidatableFactory = validationProviders => new ValidatableObjectAdapter(validationProviders);

        private readonly Dictionary<Type, DataAnnotationsValidatableObjectAdapterFactory> validatableFactories = new Dictionary<Type, DataAnnotationsValidatableObjectAdapterFactory>();

        /// <summary>
        /// Gets the validators for the model using the metadata, the validator providers, and a list of attributes.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="validatorProviders">An enumeration of validator providers.</param>
        /// <param name="attributes">The list of attributes.</param>
        /// <returns>The validators for the model.</returns>
        protected override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders, IEnumerable<Attribute> attributes)
        {
            if (metadata == null)
            {
                throw Error.ArgumentNull("metadata");
            }

            List<ModelValidator> results = new List<ModelValidator>();

            // Produce a validator for each validation attribute we find
            foreach (ValidationAttribute attribute in attributes.OfType<ValidationAttribute>())
            {
                DataAnnotationsModelValidationFactory factory;
                if (!this.attributeFactories.TryGetValue(attribute.GetType(), out factory))
                {
                    factory = this.defaultAttributeFactory;
                }

                results.Add(factory(validatorProviders, attribute));
            }

            // Produce a validator if the type supports IValidatableObject
            if (typeof(IValidatableObject).IsAssignableFrom(metadata.ModelType))
            {
                DataAnnotationsValidatableObjectAdapterFactory factory;
                if (!this.validatableFactories.TryGetValue(metadata.ModelType, out factory))
                {
                    factory = this.defaultValidatableFactory;
                }

                results.Add(factory(validatorProviders));
            }

            return results;
        }
    }
}
