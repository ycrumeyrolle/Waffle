namespace CommandProcessing.Filters
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using CommandProcessing.Internal;

    /// <summary>Represents the base class for handler-filter attributes.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public abstract class FilterAttribute : Attribute, IFilter
    {
        private static readonly ConcurrentDictionary<Type, bool> AttributeUsageCache = new ConcurrentDictionary<Type, bool>();

        /// <summary>
        /// Gets a value indicating whether multiple filters are allowed.
        /// </summary>
        /// <returns>
        /// true if multiple filters are allowed; otherwise, false.
        /// </returns>
        /// <value>
        /// The allow multiple.
        /// </value>
        public virtual bool AllowMultiple
        {
            get
            {
                return FilterAttribute.AllowsMultiple(this.GetType());
            }
        }

        private static bool AllowsMultiple(Type attributeType)
        {
            return FilterAttribute.AttributeUsageCache.GetOrAdd(attributeType, type => type.GetCustomAttributes<AttributeUsageAttribute>(true).First().AllowMultiple);
        }
    }
}