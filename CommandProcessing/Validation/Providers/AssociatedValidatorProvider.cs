namespace CommandProcessing.Validation.Providers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using CommandProcessing.Internal;
    using CommandProcessing.Metadata;
    using CommandProcessing.Validation;

    /// <summary>
    /// Provides an abstract class for classes that implement a validation provider.
    /// </summary>
    public abstract class AssociatedValidatorProvider : ModelValidatorProvider
    {
        /// <summary>
        /// Gets a type descriptor for the specified type.
        /// </summary>
        /// <param name="type">The type of the validation provider.</param>
        /// <returns>A type descriptor for the specified type.</returns>
        protected virtual ICustomTypeDescriptor GetTypeDescriptor(Type type)
        {
            return TypeDescriptorHelper.Get(type);
        }

        /// <summary>
        /// Gets the validators for the model using the metadata and validator providers.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="validatorProviders">An enumeration of validator providers.</param>
        /// <returns>The validators for the model.</returns>
        public sealed override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders)
        {
            if (metadata == null)
            {
                throw Error.ArgumentNull("metadata");
            }

            if (validatorProviders == null)
            {
                throw Error.ArgumentNull("validatorProviders");
            }

            if (metadata.ContainerType != null && !string.IsNullOrEmpty(metadata.PropertyName))
            {
                return this.GetValidatorsForProperty(metadata, validatorProviders);
            }

            return this.GetValidatorsForType(metadata, validatorProviders);
        }

        /// <summary>
        /// Gets the validators for the model using the metadata, the validator providers, and a list of attributes.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="validatorProviders">An enumeration of validator providers.</param>
        /// <param name="attributes">The list of attributes.</param>
        /// <returns>The validators for the model.</returns>
        protected abstract IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders, IEnumerable<Attribute> attributes);

        private IEnumerable<ModelValidator> GetValidatorsForProperty(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders)
        {
            ICustomTypeDescriptor typeDescriptor = this.GetTypeDescriptor(metadata.ContainerType);
            PropertyDescriptor property = typeDescriptor.GetProperties().Find(metadata.PropertyName, true);
            if (property == null)
            {
                throw Error.Argument("metadata", Resources.Common_PropertyNotFound, metadata.ContainerType, metadata.PropertyName);
            }

            return this.GetValidators(metadata, validatorProviders, property.Attributes.OfType<Attribute>());
        }

        private IEnumerable<ModelValidator> GetValidatorsForType(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders)
        {
            return this.GetValidators(metadata, validatorProviders, this.GetTypeDescriptor(metadata.ModelType).GetAttributes().Cast<Attribute>());
        }
    }
}
