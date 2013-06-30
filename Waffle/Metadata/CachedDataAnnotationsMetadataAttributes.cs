namespace Waffle.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using Waffle.Caching;

    /// <summary>
    /// Provides prototype cache data for <see cref="CachedModelMetadata{TPrototypeCache}"/>.
    /// </summary>
    public class CachedDataAnnotationsMetadataAttributes
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachedDataAnnotationsMetadataAttributes"/> class.
        /// </summary>
        /// <param name="attributes">The attributes that provides data for the initialization.</param>
        public CachedDataAnnotationsMetadataAttributes(IEnumerable<Attribute> attributes)
        {
            Contract.Requires(attributes != null);
            this.CacheAttributes(attributes);
        }

        /// <summary>
        /// Gets or sets the metadata display attribute. 
        /// </summary>
        /// <value>The metadata display attribute.</value>
        public DisplayAttribute Display { get; protected set; }

        /// <summary>
        /// Gets or sets the metadata display attribute. 
        /// </summary>
        /// <value>The metadata display attribute.</value>
        public IgnoreCachingAttribute IgnoreCaching { get; protected set; }

        private void CacheAttributes(IEnumerable<Attribute> attributes)
        {
            Contract.Requires(attributes != null); 
            this.Display = attributes.OfType<DisplayAttribute>().FirstOrDefault();
            this.IgnoreCaching = attributes.OfType<IgnoreCachingAttribute>().FirstOrDefault();
        }
    }
}