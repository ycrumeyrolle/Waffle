namespace CommandProcessing.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using CommandProcessing.Internal;

    /// <summary>
    /// Provides a container for common metadata, for the <see cref="ModelMetadataProvider"/> class.
    /// </summary>
    public class ModelMetadata
    {
        private readonly Type containerType;
        private readonly Type modelType;
        private readonly string propertyName;

        // Explicit backing store for the things we want initialized by default, so don't have to call
        // the protected virtual setters of an auto-generated property.
        private object model;
        private Func<object> modelAccessor;
        private IEnumerable<ModelMetadata> properties;
        private Type realModelType;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelMetadata"/> class. 
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="containerType">The type of the container.</param>
        /// <param name="modelAccessor">The model accessor.</param>
        /// <param name="modelType">The type of the model.</param>
        /// <param name="propertyName">The name of the property.</param>
        public ModelMetadata(ModelMetadataProvider provider, Type containerType, Func<object> modelAccessor, Type modelType, string propertyName)
        {
            if (provider == null)
            {
                throw Error.ArgumentNull("provider");
            }

            if (modelType == null)
            {
                throw Error.ArgumentNull("modelType");
            }

            this.Provider = provider;

            this.containerType = containerType;
            this.modelAccessor = modelAccessor;
            this.modelType = modelType;
            this.propertyName = propertyName;
        }

        /// <summary>
        /// Gets or sets the type of the container for the model.
        /// </summary>
        /// <value>The type of the container for the model.</value>
        public Type ContainerType
        {
            get { return this.containerType; }
        }

        /// <summary>
        /// Gets or sets the description of the model.
        /// </summary>
        /// <value>The description of the model. The default value is null.</value>
        public virtual string Description { get; set; }
       
        /// <summary>
        /// Gets or sets a value that indicates whether the model is a complex type.
        /// </summary>
        /// <value>A value that indicates whether the model is considered a complex.</value>
        public virtual bool IsComplexType
        {
            get 
            {
                Contract.Requires(this.ModelType != null);

                return !TypeHelper.HasStringConverter(this.ModelType); 
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the model is ignored in caching mecanism. 
        /// </summary>
        /// <value>A value that indicates whether the model is ignored in caching mecanism.</value>
        public virtual bool IgnoreCaching { get; set; }
        
        /// <summary>
        /// Gets the value of the model.
        /// </summary>
        /// <value>The model value.</value>
        /// <remarks>The model value can be null.</remarks>
        public object Model
        {
            get
            {
                if (this.modelAccessor != null)
                {
                    this.model = this.modelAccessor();
                    this.modelAccessor = null;
                }

                return this.model;
            }

            set
            {
                this.model = value;
                this.modelAccessor = null;
                this.properties = null;
                this.realModelType = null;
            }
        }

        /// <summary>
        /// Gets the type of the model.
        /// </summary>
        /// <value>The type of the model.</value>
        public Type ModelType
        {
            get { return this.modelType; }
        }

        /// <summary>
        /// Gets a collection of model metadata objects that describe the properties of the model.
        /// </summary>
        /// <value>A collection of model metadata objects that describe the properties of the model.</value>
        public IEnumerable<ModelMetadata> Properties
        {
            get
            {
                if (this.properties == null)
                {
                    this.properties = this.Provider.GetMetadataForProperties(this.Model, this.RealModelType);
                }

                return this.properties;
            }
        }

        /// <summary>
        /// Gets the property name.
        /// </summary>
        /// <value>The property name.</value>
        public string PropertyName
        {
            get { return this.propertyName; }
        }

        /// <summary>
        /// Gets or sets the provider.
        /// </summary>
        /// /<value>The provider.</value>
        protected ModelMetadataProvider Provider { get; set; }

        private Type RealModelType
        {
            get
            {
                if (this.realModelType == null)
                {
                    this.realModelType = this.ModelType;

                    // Don't call GetType() if the model is Nullable<T>, because it will
                    // turn Nullable<T> into T for non-null values
                    if (this.Model != null && !TypeHelper.IsNullableValueType(this.ModelType))
                    {
                        this.realModelType = this.Model.GetType();
                    }
                }

                return this.realModelType;
            }
        }

        /// <summary>
        /// Gets the display name for the model.
        /// </summary>
        /// <returns>The display name for the model.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "The method is a delegating helper to choose among multiple property values")]
        public string GetDisplayName()
        {
            return this.PropertyName ?? this.ModelType.Name;
        }
    }
}
