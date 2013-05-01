namespace CommandProcessing.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Caching;
    using System.Security.Cryptography;
    using System.Text;
    using CommandProcessing.Filters;
    using CommandProcessing.Internal;
    using CommandProcessing.Metadata;

    /// <summary>
    /// Represents a filter to cache command result.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "NoCacheAttri")]
    public class CacheAttribute : HandlerFilterAttribute
    {
        private const string CacheKey = "__CacheAttribute";

        /// <summary>
        /// Vary by everything.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Params", Justification = "Params is in analogy with \"VaryByParams\" from HTTP.")]
        public const string VaryByParamsAll = "*";

        /// <summary>
        /// Vary by nothing.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Params", Justification = "Params is in analogy with \"VaryByParams\" from HTTP.")]
        public const string VaryByParamsNone = "none";

        private string varyByParams = VaryByParamsAll;

        private readonly ObjectCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheAttribute"/> class.
        /// </summary>
        public CacheAttribute()
            : this(MemoryCache.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheAttribute"/> class.
        /// </summary>
        /// <param name="cache">The <see cref="ObjectCache"/></param>
        internal CacheAttribute(ObjectCache cache)
        {
            this.cache = cache;
        }

        /// <summary>
        /// Gets or sets the cache duration, in seconds.
        /// </summary>
        /// <value>The cache duration, in seconds. </value>
        public int Duration { get; set; }

        /// <summary>
        /// Gets or sets a semi colon delimited list of string parameters that the cache uses to vary the cache entry.
        /// </summary>
        /// <value>The vary-by-param value.</value>
        /// <remarks>
        /// Accepted values are : 
        /// <list type="bullet">
        /// <item>* : vary by everything</item>
        /// <item>none : vary by nothig</item>
        /// <item>Any other string : vary by the specified params, separated by a semi colon.</item>
        /// </list>
        /// Default value is '*'.
        /// If a param is not found, it is ignored.
        /// </remarks>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Params", Justification = "Params is in analogy with \"VaryByParams\" from HTTP.")]
        public string VaryByParams
        {
            get
            {
                return this.varyByParams;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw Error.PropertyNull();
                }

                this.varyByParams = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the cache must vary by user.
        /// </summary>
        /// <value><c>true</c> if the cache is specific for each user ; <c>false</c> if the cache is shared among users.</value>
        public bool VaryByUser { get; set; }

        /// <summary>
        /// Occurs before the handle method is invoked.
        /// </summary>
        /// <param name="handlerContext">The handler context.</param>
        public override void OnCommandExecuting(HandlerContext handlerContext)
        {
            if (handlerContext == null)
            {
                throw Error.ArgumentNull("handlerContext");
            }

            if (handlerContext.Descriptor.GetCustomAttributes<NoCacheAttribute>().Count > 0)
            {
                return;
            }

            string key = this.GetUniqueId(handlerContext);
            CacheEntry entry = this.cache.Get(key) as CacheEntry;
            if (entry != null)
            {
                handlerContext.Result = entry.Value;
                return;
            }

            handlerContext.Items[CacheKey] = key;
        }

        /// <summary>
        /// Occurs after the handle method is invoked.
        /// </summary>
        /// <param name="handlerExecutedContext">The handler executed context.</param>
        public override void OnCommandExecuted(HandlerExecutedContext handlerExecutedContext)
        {
            if (handlerExecutedContext == null)
            {
                throw Error.ArgumentNull("handlerExecutedContext");
            }

            if (handlerExecutedContext.Exception != null)
            {
                return;
            }

            if (handlerExecutedContext.HandlerContext.Descriptor.GetCustomAttributes<NoCacheAttribute>().Count > 0)
            {
                return;
            }
            
            if (!handlerExecutedContext.HandlerContext.Items.ContainsKey(CacheKey))
            {
                return;
            }

            string key = handlerExecutedContext.HandlerContext.Items[CacheKey] as string;
            if (key == null)
            {
                return;
            }

            DateTimeOffset expiration = this.CreateExpiration();

            this.cache.Add(key, new CacheEntry(handlerExecutedContext.Result), expiration); 
        }

        private DateTimeOffset CreateExpiration()
        {
            if (this.Duration == 0)
            {
                return DateTimeOffset.MaxValue;
            }

            return DateTimeOffset.UtcNow.AddSeconds(this.Duration);
        }

        private string GetUniqueId(HandlerContext filterContext)
        {
            StringBuilder uniqueIdBuilder = new StringBuilder();
            
            // Unique ID of the handler description
            AppendPartToUniqueIdBuilder(uniqueIdBuilder, filterContext.Descriptor.HandlerType);

            if (this.VaryByUser && filterContext.User != null)
            {
                AppendPartToUniqueIdBuilder(uniqueIdBuilder, filterContext.User.Identity.Name);
            }
            
            // Unique ID from the VaryByParams settings, if any
            uniqueIdBuilder.Append(this.GetUniqueIdFromCommand(filterContext));

            // The key is typically too long to be useful, so we use a cryptographic hash
            // as the actual key (better randomization and key distribution, so small vary
            // values will generate dramtically different keys).
            using (SHA256 sha = SHA256.Create())
            {
                return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(uniqueIdBuilder.ToString())));
            }
        }
        
        // Generate a unique ID of normalized key names + key values
        private string GetUniqueIdFromCommand(HandlerContext filterContext)
        {
            IModelFlattener flattener = filterContext.Configuration.Services.GetModelFlattener();
            ModelMetadataProvider metadataProvider = filterContext.Configuration.Services.GetModelMetadataProvider();
            ModelDictionary parameters = flattener.Flatten(filterContext.Command, filterContext.Command.GetType(), metadataProvider, string.Empty);
            IEnumerable<string> keys = SplitVaryByParam(this.VaryByParams);

            keys = (keys ?? parameters.Keys).OrderBy(key => key, StringComparer.Ordinal);

            return CreateUniqueId(keys.Concat(keys.Select(key => parameters.ContainsKey(key) ? parameters[key] : null)));
        }

        private static IEnumerable<string> SplitVaryByParam(string varyByParam)
        {
            if (string.Equals(varyByParam, VaryByParamsNone, StringComparison.OrdinalIgnoreCase))
            {
                // Vary by nothing
                return Enumerable.Empty<string>();
            }

            if (string.Equals(varyByParam, VaryByParamsAll, StringComparison.OrdinalIgnoreCase))
            {
                // Vary by everything
                return null;
            }

            // Vary by specific parameters
            return from part in varyByParam.Split(';')
                   let trimmed = part.Trim()
                   where !string.IsNullOrEmpty(trimmed)
                   select trimmed;
        }

        private static void AppendPartToUniqueIdBuilder(StringBuilder builder, object part)
        {
            if (part == null)
            {
                builder.Append("[-1]");
            }
            else
            {
                string partString = Convert.ToString(part, CultureInfo.InvariantCulture);
                builder.AppendFormat("[{0}]{1}", partString.Length, partString);
            }
        }
        
        private static string CreateUniqueId(IEnumerable<object> parts)
        {
            // returns a unique string made up of the pieces passed in
            StringBuilder builder = new StringBuilder();
            foreach (object part in parts)
            {
                AppendPartToUniqueIdBuilder(builder, part);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Encapsulates object to cache. This allows to store any object even if it is null.
        /// </summary>
        internal class CacheEntry
        {
            private readonly object value;

            public CacheEntry(object value)
            {
                this.value = value;
            }

            public object Value
            {
                get
                {
                    return this.value;
                }
            }
        }
    }
}
