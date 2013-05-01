namespace CommandProcessing.Validation.Providers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using CommandProcessing.Internal;
    using CommandProcessing.Metadata;
    using CommandProcessing.Validation;

    public abstract class AssociatedValidatorProvider : ModelValidatorProvider
    {
        protected virtual ICustomTypeDescriptor GetTypeDescriptor(Type type)
        {
            return TypeDescriptorHelper.Get(type);
        }

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
