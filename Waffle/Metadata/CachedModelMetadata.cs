namespace Waffle.Metadata
{
    using System;
    using System.Diagnostics.Contracts;
    using Waffle.Internal;

    /// <summary> 
    /// This class assumes that model metadata is expensive to create, and allows the user to
    /// stash a cache object that can be copied around as a prototype to make creation and
    /// computation quicker. It delegates the retrieval of values to getter methods, the results
    /// of which are cached on a per-metadata-instance basis.
    ///
    /// This allows flexible caching strategies: either caching the source of information across
    /// instances or caching of the actual information itself, depending on what the developer
    /// decides to put into the prototype cache.
    /// </summary>
    /// <typeparam name="TPrototypeCache">The type of the prototype.</typeparam>
    public abstract class CachedModelMetadata<TPrototypeCache> : ModelMetadata
    {
        private string description;

        private bool descriptionComputed;

        private bool isComplexType;

        private bool isComplexTypeComputed;

        private bool ignoreCaching;

        private bool ignoreCachingComputed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedDataAnnotationsMetadataAttributes"/> class.
        /// </summary>
        /// <param name="prototype">The prototype.</param>
        /// <param name="modelAccessor">The model accessor.</param>
        /// <remarks>Constructor for creating real instances of the metadata class based on a prototype.</remarks>
        protected CachedModelMetadata(CachedModelMetadata<TPrototypeCache> prototype, Func<object> modelAccessor)
            : base(prototype.Provider, prototype.ContainerType, modelAccessor, prototype.ModelType, prototype.PropertyName)
        {
            if (prototype == null)
            {
                throw Error.ArgumentNull("prototype");
            }

            this.PrototypeCache = prototype.PrototypeCache;

            this.isComplexType = prototype.IsComplexType;
            this.isComplexTypeComputed = true;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CachedDataAnnotationsMetadataAttributes"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="containerType">The type of container.</param>
        /// <param name="modelType">The type of the model.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="prototypeCache">The prototype cache.</param>
        /// <remarks>Constructor for creating the prototype instances of the metadata class.</remarks>
        protected CachedModelMetadata(DataAnnotationsModelMetadataProvider provider, Type containerType, Type modelType, string propertyName, TPrototypeCache prototypeCache)
            : base(provider, containerType, null, modelType, propertyName)
        {
            this.PrototypeCache = prototypeCache;
        }
        
        /// <summary>
        /// Gets or sets the description of the model.
        /// </summary>
        /// <value>The description of the model. The default value is null.</value>
        public sealed override string Description
        {
            get
            {
                if (!this.descriptionComputed)
                {
                    this.description = this.ComputeDescription();
                    this.descriptionComputed = true;
                }

                return this.description;
            }

            set
            {
                this.description = value;
                this.descriptionComputed = true;
            }
        }
        
        /// <summary>
        /// Gets or sets a value that indicates whether the model is a complex type.
        /// </summary>
        /// <value>A value that indicates whether the model is considered a complex.</value>
        public sealed override bool IsComplexType
        {
            get
            {
                if (!this.isComplexTypeComputed)
                {
                    this.isComplexType = this.ComputeIsComplexType();
                    this.isComplexTypeComputed = true;
                }

                return this.isComplexType;
            }
        }

        /// <summary>
        /// Gets or sets the description of the model.
        /// </summary>
        /// <value>The description of the model. The default value is null.</value>
        public sealed override bool IgnoreCaching
        {
            get
            {
                if (!this.ignoreCachingComputed)
                {
                    this.ignoreCaching = this.ComputeIgnoreCaching();
                    this.ignoreCachingComputed = true;
                }

                return this.ignoreCaching;
            }

            set
            {
                this.ignoreCaching = value;
                this.ignoreCachingComputed = true;
            }
        }

        /// <summary>
        /// Gets or sets the prototype cache.
        /// </summary>
        /// <value>The prototype cache.</value>
        protected TPrototypeCache PrototypeCache { get; set; }

        /// <summary>
        /// Computes the property Description.
        /// </summary>
        /// <returns>The property Description.</returns>
        protected virtual string ComputeDescription()
        {
            return base.Description;
        }

        /// <summary>
        /// Computes the property IsComplexType.
        /// </summary>
        /// <returns>The property IsComplexType.</returns>
        private bool ComputeIsComplexType()
        {
            Contract.Requires(this.ModelType != null);
            return base.IsComplexType;
        }

        /// <summary>
        /// Computes the property IgnoreCaching.
        /// </summary>
        /// <returns>The property IgnoreCaching.</returns>
        protected virtual bool ComputeIgnoreCaching()
        {
            return base.IgnoreCaching;
        }
    }
}