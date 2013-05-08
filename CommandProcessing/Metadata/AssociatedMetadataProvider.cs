namespace CommandProcessing.Metadata
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Reflection;
    using System.Reflection.Emit;
    using CommandProcessing.Internal;

    /// <summary>
    /// Provides an abstract class to implement a metadata provider.
    /// </summary>
    /// <typeparam name="TModelMetadata">The type of the model metadata.</typeparam>
    public abstract class AssociatedMetadataProvider<TModelMetadata> : ModelMetadataProvider
        where TModelMetadata : ModelMetadata
    {
        private readonly ConcurrentDictionary<Type, TypeInformation> typeInfoCache = new ConcurrentDictionary<Type, TypeInformation>();

        /// <summary>
        /// Retrieves a list of properties for the model.
        /// </summary>
        /// <param name="container">The model container.</param>
        /// <param name="containerType">The type of the container.  </param>
        /// <returns>A list of properties for the model.</returns>
        public sealed override IEnumerable<ModelMetadata> GetMetadataForProperties(object container, Type containerType)
        {
            if (containerType == null)
            {
                throw Error.ArgumentNull("containerType");
            }

            return this.GetMetadataForPropertiesImpl(container, containerType);
        }

        private IEnumerable<ModelMetadata> GetMetadataForPropertiesImpl(object container, Type containerType)
        {
            TypeInformation typeInfo = this.GetTypeInformation(containerType);
            foreach (KeyValuePair<string, PropertyInformation> kvp in typeInfo.Properties)
            {
                PropertyInformation propertyInfo = kvp.Value;
                Func<object> modelAccessor = null;
                if (container != null)
                {
                    Func<object, object> propertyGetter = propertyInfo.ValueAccessor;
                    modelAccessor = () => propertyGetter(container);
                }

                yield return this.CreateMetadataFromPrototype(propertyInfo.Prototype, modelAccessor);
            }
        }

        /// <summary>
        /// Returns the metadata for the specified property using the type of the model.
        /// </summary>
        /// <param name="modelAccessor">The model accessor.</param>
        /// <param name="modelType">The type of the container.</param>
        /// <returns>The metadata for the specified property.</returns>
        public sealed override ModelMetadata GetMetadataForType(Func<object> modelAccessor, Type modelType)
        {
            if (modelType == null)
            {
                throw Error.ArgumentNull("modelType");
            }

            TModelMetadata prototype = this.GetTypeInformation(modelType).Prototype;
            return this.CreateMetadataFromPrototype(prototype, modelAccessor);
        }

        /// <summary>
        /// Override for creating the prototype metadata (without the accessor).
        /// </summary>
        /// <param name="attributes">The attributes list.</param>
        /// <param name="containerType">The type of the container.</param>
        /// <param name="modelType">The type of the container.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The prototype metadata.</returns>
        protected abstract TModelMetadata CreateMetadataPrototype(IEnumerable<Attribute> attributes, Type containerType, Type modelType, string propertyName);

        /// <summary>
        /// When overridden in a derived class, creates the model metadata for the property using the specified prototype.
        /// </summary>
        /// <param name="prototype">The prototype from which to create the model metadata.</param>
        /// <param name="modelAccessor">The model accessor.</param>
        /// <returns>The model metadata for the property.</returns>
        protected abstract TModelMetadata CreateMetadataFromPrototype(TModelMetadata prototype, Func<object> modelAccessor);

        private TypeInformation GetTypeInformation(Type type)
        {
            // This retrieval is implemented as a TryGetValue/TryAdd instead of a GetOrAdd to avoid the performance cost of creating instance delegates
            TypeInformation typeInfo;
            if (!this.typeInfoCache.TryGetValue(type, out typeInfo))
            {
                typeInfo = this.CreateTypeInformation(type);
                this.typeInfoCache.TryAdd(type, typeInfo);
            }

            return typeInfo;
        }

        private TypeInformation CreateTypeInformation(Type type)
        {
            TypeInformation info = new TypeInformation();
            ICustomTypeDescriptor typeDescriptor = TypeDescriptorHelper.Get(type);
            info.TypeDescriptor = typeDescriptor;
            info.Prototype = this.CreateMetadataPrototype(AsAttributes(typeDescriptor.GetAttributes()), null, type, null);

            Dictionary<string, PropertyInformation> properties = new Dictionary<string, PropertyInformation>();
            foreach (PropertyDescriptor property in typeDescriptor.GetProperties())
            {
                // Avoid re-generating a property descriptor if one has already been generated for the property name   
                if (!properties.ContainsKey(property.Name))
                {
                    properties.Add(property.Name, this.CreatePropertyInformation(type, property));
                }
            }

            info.Properties = properties;

            return info;
        }

        private PropertyInformation CreatePropertyInformation(Type containerType, PropertyDescriptor property)
        {
            PropertyInformation info = new PropertyInformation();
            info.ValueAccessor = CreatePropertyValueAccessor(property);
            info.Prototype = this.CreateMetadataPrototype(AsAttributes(property.Attributes), containerType, property.PropertyType, property.Name);
            return info;
        }

        // Optimization: yield provides much better performance than the LINQ .Cast<Attribute>() in this case
        private static IEnumerable<Attribute> AsAttributes(IEnumerable attributes)
        {
            foreach (object attribute in attributes)
            {
                yield return attribute as Attribute;
            }
        }

        private static Func<object, object> CreatePropertyValueAccessor(PropertyDescriptor property)
        {
            Type declaringType = property.ComponentType;
            if (declaringType.IsVisible)
            {
                string propertyName = property.Name;
                PropertyInfo propertyInfo = declaringType.GetProperty(propertyName, property.PropertyType);

                if (propertyInfo != null && propertyInfo.CanRead)
                {
                    MethodInfo getMethodInfo = propertyInfo.GetGetMethod();
                    if (getMethodInfo != null)
                    {
                        return CreateDynamicValueAccessor(getMethodInfo, declaringType, propertyName);
                    }
                }
            }

            // If either the type isn't public or we can't find a public getter, use the slow Reflection path
            return container => property.GetValue(container);
        }

        // Uses Lightweight Code Gen to generate a tiny delegate that gets the property value
        // This is an optimization to avoid having to go through the much slower System.Reflection APIs
        // e.g. generates (object o) => (Person)o.Id
        private static Func<object, object> CreateDynamicValueAccessor(MethodInfo getMethodInfo, Type declaringType, string propertyName)
        {
            Contract.Assert(getMethodInfo != null && getMethodInfo.IsPublic && !getMethodInfo.IsStatic);

            Type propertyType = getMethodInfo.ReturnType;
            DynamicMethod dynamicMethod = new DynamicMethod("Get" + propertyName + "From" + declaringType.Name, typeof(object), new[] { typeof(object) });
            ILGenerator ilg = dynamicMethod.GetILGenerator();

            // Load the container onto the stack, convert from object => declaring type for the property
            ilg.Emit(OpCodes.Ldarg_0);
            if (declaringType.IsValueType)
            {
                ilg.Emit(OpCodes.Unbox, declaringType);
            }
            else
            {
                ilg.Emit(OpCodes.Castclass, declaringType);
            }

            // if declaring type is value type, we use Call : structs don't have inheritance
            // if get method is sealed or isn't virtual, we use Call : it can't be overridden
            if (declaringType.IsValueType || !getMethodInfo.IsVirtual || getMethodInfo.IsFinal)
            {
                ilg.Emit(OpCodes.Call, getMethodInfo);
            }
            else
            {
                ilg.Emit(OpCodes.Callvirt, getMethodInfo);
            }

            // Box if the property type is a value type, so it can be returned as an object
            if (propertyType.IsValueType)
            {
                ilg.Emit(OpCodes.Box, propertyType);
            }

            // Return property value
            ilg.Emit(OpCodes.Ret);

            return (Func<object, object>)dynamicMethod.CreateDelegate(typeof(Func<object, object>));
        }

        private class TypeInformation
        {
            public ICustomTypeDescriptor TypeDescriptor { get; set; }

            public TModelMetadata Prototype { get; set; }

            public Dictionary<string, PropertyInformation> Properties { get; set; }
        }

        private class PropertyInformation
        {
            public Func<object, object> ValueAccessor { get; set; }

            public TModelMetadata Prototype { get; set; }
        }
    }
}