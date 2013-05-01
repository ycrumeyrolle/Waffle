namespace CommandProcessing.Validation.Providers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Reflection;
    using CommandProcessing.Filters;
    using CommandProcessing.Internal;
    using CommandProcessing.Metadata;
    using CommandProcessing.Validation;
    using CommandProcessing.Validation.Validators;

    // A factory for validators based on ValidationAttribute
    public delegate ModelValidator DataAnnotationsModelValidationFactory(IEnumerable<ModelValidatorProvider> validatorProviders, ValidationAttribute attribute);

    // A factory for validators based on IValidatableObject
    public delegate ModelValidator DataAnnotationsValidatableObjectAdapterFactory(IEnumerable<ModelValidatorProvider> validatorProviders);

    /// <summary>
    /// An implementation of <see cref="ModelValidatorProvider"/> which providers validators
    /// for attributes which derive from <see cref="ValidationAttribute"/>. It also provides
    /// a validator for types which implement <see cref="IValidatableObject"/>.
    /// </summary>
    public class DataAnnotationsModelValidatorProvider : AssociatedValidatorProvider
    {
        // Factories for validation attributes
        private DataAnnotationsModelValidationFactory defaultAttributeFactory = (validationProviders, attribute) => new DataAnnotationsModelValidator(validationProviders, attribute);

        private readonly Dictionary<Type, DataAnnotationsModelValidationFactory> attributeFactories = new Dictionary<Type, DataAnnotationsModelValidationFactory>();

        // Factories for IValidatableObject models
        private DataAnnotationsValidatableObjectAdapterFactory defaultValidatableFactory = (validationProviders) => new ValidatableObjectAdapter(validationProviders);

        private readonly Dictionary<Type, DataAnnotationsValidatableObjectAdapterFactory> validatableFactories = new Dictionary<Type, DataAnnotationsValidatableObjectAdapterFactory>();

        protected override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders, IEnumerable<Attribute> attributes)
        {
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

        public void RegisterAdapter(Type attributeType, Type adapterType)
        {
            ValidateAttributeType(attributeType);
            ValidateAttributeAdapterType(adapterType);
            ConstructorInfo constructor = GetAttributeAdapterConstructor(attributeType, adapterType);

            this.attributeFactories[attributeType] = (context, attribute) => (ModelValidator)constructor.Invoke(new object[] { context, attribute });
        }

        public void RegisterAdapterFactory(Type attributeType, DataAnnotationsModelValidationFactory factory)
        {
            ValidateAttributeType(attributeType);
            ValidateAttributeFactory(factory);

            this.attributeFactories[attributeType] = factory;
        }

        public void RegisterDefaultAdapter(Type adapterType)
        {
            ValidateAttributeAdapterType(adapterType);
            ConstructorInfo constructor = GetAttributeAdapterConstructor(typeof(ValidationAttribute), adapterType);

            this.defaultAttributeFactory = (context, attribute) => (ModelValidator)constructor.Invoke(new object[] { context, attribute });
        }

        public void RegisterDefaultAdapterFactory(DataAnnotationsModelValidationFactory factory)
        {
            ValidateAttributeFactory(factory);

            this.defaultAttributeFactory = factory;
        }

        // Helpers 
        private static ConstructorInfo GetAttributeAdapterConstructor(Type attributeType, Type adapterType)
        {
            ConstructorInfo constructor = adapterType.GetConstructor(new[] { typeof(IEnumerable<ModelValidatorProvider>), attributeType });
            if (constructor == null)
            {
                throw Error.Argument("adapterType", Resources.DataAnnotationsModelValidatorProvider_ConstructorRequirements, adapterType.Name, typeof(ModelMetadata).Name, "IEnumerable<" + typeof(ModelValidatorProvider).Name + ">", attributeType.Name);
            }

            return constructor;
        }

        private static void ValidateAttributeAdapterType(Type adapterType)
        {
            if (adapterType == null)
            {
                throw Error.ArgumentNull("adapterType");
            }

            if (!typeof(ModelValidator).IsAssignableFrom(adapterType))
            {
                throw Error.Argument("adapterType", Resources.Common_TypeMustDeriveFromType, adapterType.Name, typeof(ModelValidator).Name);
            }
        }

        private static void ValidateAttributeType(Type attributeType)
        {
            if (attributeType == null)
            {
                throw Error.ArgumentNull("attributeType");
            }

            if (!typeof(ValidationAttribute).IsAssignableFrom(attributeType))
            {
                throw Error.Argument("attributeType", Resources.Common_TypeMustDeriveFromType, attributeType.Name, typeof(ValidationAttribute).Name);
            }
        }

        private static void ValidateAttributeFactory(DataAnnotationsModelValidationFactory factory)
        {
            if (factory == null)
            {
                throw Error.ArgumentNull("factory");
            }
        }

        /// <summary>
        /// Registers an adapter type for the given <paramref name="modelType"/>, which must
        /// implement <see cref="IValidatableObject"/>. The adapter type must derive from
        /// <see cref="ModelValidator"/> and it must contain a public constructor
        /// which takes two parameters of types <see cref="ModelMetadata"/> and
        /// <see cref="HandlerContext"/>.
        /// </summary>
        public void RegisterValidatableObjectAdapter(Type modelType, Type adapterType)
        {
            ValidateValidatableModelType(modelType);
            ValidateValidatableAdapterType(adapterType);
            ConstructorInfo constructor = GetValidatableAdapterConstructor(adapterType);

            this.validatableFactories[modelType] = context => (ModelValidator)constructor.Invoke(new object[] { context });
        }

        /// <summary>
        /// Registers an adapter factory for the given <paramref name="modelType"/>, which must
        /// implement <see cref="IValidatableObject"/>.
        /// </summary>
        public void RegisterValidatableObjectAdapterFactory(Type modelType, DataAnnotationsValidatableObjectAdapterFactory factory)
        {
            ValidateValidatableModelType(modelType);
            ValidateValidatableFactory(factory);

            this.validatableFactories[modelType] = factory;
        }

        /// <summary>
        /// Registers the default adapter type for objects which implement
        /// <see cref="IValidatableObject"/>. The adapter type must derive from
        /// <see cref="ModelValidator"/> and it must contain a public constructor
        /// which takes two parameters of types <see cref="ModelMetadata"/> and
        /// <see cref="HandlerContext"/>.
        /// </summary>
        public void RegisterDefaultValidatableObjectAdapter(Type adapterType)
        {
            ValidateValidatableAdapterType(adapterType);
            ConstructorInfo constructor = GetValidatableAdapterConstructor(adapterType);

            this.defaultValidatableFactory = context => (ModelValidator)constructor.Invoke(new object[] { context });
        }

        /// <summary>
        /// Registers the default adapter factory for objects which implement
        /// <see cref="IValidatableObject"/>.
        /// </summary>
        public void RegisterDefaultValidatableObjectAdapterFactory(DataAnnotationsValidatableObjectAdapterFactory factory)
        {
            ValidateValidatableFactory(factory);

            this.defaultValidatableFactory = factory;
        }

        // Helpers 
        private static ConstructorInfo GetValidatableAdapterConstructor(Type adapterType)
        {
            ConstructorInfo constructor = adapterType.GetConstructor(new[] { typeof(IEnumerable<ModelValidatorProvider>) });
            if (constructor == null)
            {
                throw Error.Argument("adapterType", Resources.DataAnnotationsModelValidatorProvider_ValidatableConstructorRequirements, adapterType.Name, typeof(ModelMetadata).Name, "IEnumerable<" + typeof(ModelValidatorProvider).Name + ">");
            }

            return constructor;
        }

        private static void ValidateValidatableAdapterType(Type adapterType)
        {
            if (adapterType == null)
            {
                throw Error.ArgumentNull("adapterType");
            }
            if (!typeof(ModelValidator).IsAssignableFrom(adapterType))
            {
                throw Error.Argument("adapterType", Resources.Common_TypeMustDeriveFromType, adapterType.Name, typeof(ModelValidator).Name);
            }
        }

        private static void ValidateValidatableModelType(Type modelType)
        {
            if (modelType == null)
            {
                throw Error.ArgumentNull("modelType");
            }
            if (!typeof(IValidatableObject).IsAssignableFrom(modelType))
            {
                throw Error.Argument("modelType", Resources.Common_TypeMustDeriveFromType, modelType.Name, typeof(IValidatableObject).Name);
            }
        }

        private static void ValidateValidatableFactory(DataAnnotationsValidatableObjectAdapterFactory factory)
        {
            if (factory == null)
            {
                throw Error.ArgumentNull("factory");
            }
        }
    }
}
