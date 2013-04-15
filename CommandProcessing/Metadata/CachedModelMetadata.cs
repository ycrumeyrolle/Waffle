namespace CommandProcessing.Metadata
{
    using System;
    using CommandProcessing.Internal;

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
    /// <typeparam name="TPrototypeCache"></typeparam>
    public abstract class CachedModelMetadata<TPrototypeCache> : ModelMetadata
    {
        private string description;

        private bool isComplexType;

        private bool descriptionComputed;

        private bool isComplexTypeComputed;

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

        protected TPrototypeCache PrototypeCache { get; set; }

        protected virtual string ComputeDescription()
        {
            return base.Description;
        }

        private bool ComputeIsComplexType()
        {
            return base.IsComplexType;
        }
    }
}