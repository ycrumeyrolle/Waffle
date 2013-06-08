namespace CommandProcessing.Validation
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using CommandProcessing.Internal;
    using CommandProcessing.Metadata;

    /// <summary>
    /// Recursively validate an object. 
    /// </summary>
    public class DefaultCommandValidator : ICommandValidator
    {
        private readonly ConcurrentDictionary<Type, Type> elementTypeCache = new ConcurrentDictionary<Type, Type>();

        /// <summary>
        /// Determines whether the command is valid and adds any validation errors to the command's ValidationResults.
        /// </summary>
        /// <param name="request">The <see cref="HandlerRequest"/> to be validated.</param>
        /// <returns>true if command is valid, false otherwise.</returns>
        public bool Validate(HandlerRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            ModelValidatorProvider[] validatorProviders = request.Configuration.Services.GetModelValidatorProviders();

            // Optimization : avoid validating the object graph if there are no validator providers
            if (validatorProviders.Length == 0)
            {
                return true;
            }

            ModelMetadataProvider metadataProvider = request.Configuration.Services.GetModelMetadataProvider();
            ModelMetadata metadata = metadataProvider.GetMetadataForType(() => request.Command, request.CommandType);
            ValidationContext validationContext = new ValidationContext
                {
                    MetadataProvider = metadataProvider,
                    ValidatorProviders = validatorProviders,
                    ValidatorCache = request.Configuration.Services.GetModelValidatorCache(),
                    ModelState = request.Command.ModelState,
                    Visited = new HashSet<object>(),
                    KeyBuilders = new Stack<IKeyBuilder>(),
                    Request = request,
                    RootPrefix = string.Empty
                };
            return this.ValidateNodeAndChildren(metadata, validationContext, container: null);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "See comment below")]
        private bool ValidateNodeAndChildren(ModelMetadata metadata, ValidationContext validationContext, object container)
        {
            object model;
            try
            {
                model = metadata.Model;
            }
            catch
            {
                // Retrieving the model failed - typically caused by a property getter throwing   
                // Being unable to retrieve a property is not a validation error - many properties can only be retrieved if certain conditions are met   
                // For example, Uri.AbsoluteUri throws for relative URIs but it shouldn't be considered a validation error   
                return true;
            }

            bool isValid;

            // Optimization: we don't need to recursively traverse the graph for null and primitive types
            if (model == null || TypeHelper.IsSimpleType(model.GetType()))
            {
                return ShallowValidate(metadata, validationContext, container);
            }

            // Check to avoid infinite recursion. This can happen with cycles in an object graph.
            if (validationContext.Visited.Contains(model))
            {
                return true;
            }

            validationContext.Visited.Add(model);

            // Validate the children first - depth-first traversal
            IEnumerable enumerableModel = model as IEnumerable;
            if (enumerableModel == null)
            {
                isValid = this.ValidateProperties(metadata, validationContext);
            }
            else
            {
                isValid = this.ValidateElements(enumerableModel, validationContext);
            }

            if (isValid)
            {
                // Don't bother to validate this node if children failed.
                isValid = ShallowValidate(metadata, validationContext, container);
            }

            // Pop the object so that it can be validated again in a different path
            validationContext.Visited.Remove(model);

            return isValid;
        }

        private bool ValidateProperties(ModelMetadata metadata, ValidationContext validationContext)
        {
            bool isValid = true;
            PropertyScope propertyScope = new PropertyScope();
            validationContext.KeyBuilders.Push(propertyScope);
            foreach (ModelMetadata childMetadata in validationContext.MetadataProvider.GetMetadataForProperties(metadata.Model, metadata.RealModelType))
            {
                propertyScope.PropertyName = childMetadata.PropertyName;
                if (!this.ValidateNodeAndChildren(childMetadata, validationContext, metadata.Model))
                {
                    isValid = false;
                }
            }

            validationContext.KeyBuilders.Pop();
            return isValid;
        }

        private bool ValidateElements(IEnumerable model, ValidationContext validationContext)
        {
            bool isValid = true;
            Type elementType = this.GetElementType(model.GetType());
            ModelMetadata elementMetadata = validationContext.MetadataProvider.GetMetadataForType(null, elementType);

            ElementScope elementScope = new ElementScope { Index = 0 };
            validationContext.KeyBuilders.Push(elementScope);
            foreach (object element in model)
            {
                elementMetadata.Model = element;
                if (!this.ValidateNodeAndChildren(elementMetadata, validationContext, model))
                {
                    isValid = false;
                }

                elementScope.Index++;
            }

            validationContext.KeyBuilders.Pop();
            return isValid;
        }

        // Validates a single node (not including children)
        // Returns true if validation passes successfully
        private static bool ShallowValidate(ModelMetadata metadata, ValidationContext validationContext, object container)
        {
            bool isValid = true;
            string key = null;
            IModelValidatorCache validatorCache = validationContext.ValidatorCache;
            ModelValidator[] validators;
            if (validatorCache == null)
            {
                // slow path: there is no validator cache on the configuration
                validators = metadata.GetValidators(validationContext.ValidatorProviders).AsArray();
            }
            else
            {
                validators = validatorCache.GetValidators(metadata).AsArray();
            }

            for (int index = 0; index < validators.Length; index++)
            {
                ModelValidator validator = validators[index];
                foreach (ModelValidationResult error in validator.Validate(metadata, container))
                {
                    if (key == null)
                    {
                        key = validationContext.RootPrefix;
                        foreach (IKeyBuilder keyBuilder in validationContext.KeyBuilders.Reverse())
                        {
                            key = keyBuilder.AppendTo(key);
                        }

                        // Avoid adding model errors if the model state already contains model errors for that key
                        // We can't perform this check earlier because we compute the key string only when we detect an error
                        if (!validationContext.ModelState.IsValidField(key))
                        {
                            return false;
                        }
                    }

                    validationContext.ModelState.AddModelError(key, error.Message);
                    isValid = false;
                }
            }

            return isValid;
        }

        private Type GetElementType(Type type)
        {
            Contract.Assert(typeof(IEnumerable).IsAssignableFrom(type));

            // Avoid to use reflection when it is possible
            Type elementType;
            if (this.elementTypeCache.TryGetValue(type, out elementType))
            {
                return elementType;
            }

            if (type.IsArray)
            {
                elementType = type.GetElementType();
            }

            Type[] interfaces = type.GetInterfaces();
            for (int index = 0; index < interfaces.Length; index++)
            {
                Type implementedInterface = interfaces[index];
                if (implementedInterface.IsGenericType && implementedInterface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    elementType = implementedInterface.GetGenericArguments()[0];
                    break;
                }
            }

            if (elementType == null)
            {
                elementType = typeof(object);
            }

            this.elementTypeCache.TryAdd(type, elementType);

            return elementType;
        }

        private class ValidationContext
        {
            public ModelMetadataProvider MetadataProvider { get; set; }

            public ModelValidatorProvider[] ValidatorProviders { get; set; }

            public IModelValidatorCache ValidatorCache { get; set; }

            public ModelStateDictionary ModelState { get; set; }

            public HashSet<object> Visited { get; set; }

            public Stack<IKeyBuilder> KeyBuilders { get; set; }

            public HandlerRequest Request { get; set; }

            public string RootPrefix { get; set; }
        }
    }
}
