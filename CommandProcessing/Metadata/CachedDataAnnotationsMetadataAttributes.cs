namespace CommandProcessing.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Security;

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
            this.CacheAttributes(attributes);
        }

        /// <summary>
        /// Gets or sets the metadata display attribute. 
        /// </summary>
        /// <value>The metadata display attribute.</value>
        public DisplayAttribute Display { [SecuritySafeCritical] get; [SecuritySafeCritical] protected set; }

        // [SecuritySafeCritical] because it uses several DataAnnotations attribute types
        [SecuritySafeCritical]
        private void CacheAttributes(IEnumerable<Attribute> attributes)
        {
            this.Display = attributes.OfType<DisplayAttribute>().FirstOrDefault();
        }
    }
}