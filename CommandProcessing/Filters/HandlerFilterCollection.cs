namespace CommandProcessing.Filters
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using CommandProcessing.Internal;

    /// <summary>
    /// Represents a collection of handler filters.
    /// </summary>
    public class HandlerFilterCollection : IEnumerable<FilterInfo>
    {
        private readonly List<FilterInfo> filters = new List<FilterInfo>();

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        /// <value>The number of elements in the collection.</value>
        public int Count
        {
            get
            {
                return this.filters.Count;
            }
        }

        /// <summary>
        /// Adds an item at the end of the collection.
        /// </summary>
        /// <param name="filter">The item to add to the collection.</param>
        public void Add(IFilter filter)
        {
            if (filter == null)
            {
                throw Error.ArgumentNull("filter");
            }

            this.AddInternal(new FilterInfo(filter, FilterScope.Global));
        }

        /// <summary>
        /// Removes all item in the collection.
        /// </summary>
        public void Clear()
        {
            this.filters.Clear();
        }

        /// <summary>
        /// Determines whether the collection contains the specified item.
        /// </summary>
        /// <param name="filter">The item to check.</param>
        /// <returns>true if the collection contains the specified item; otherwise, false.  </returns>
        public bool Contains(IFilter filter)
        {
            return this.filters.Any(f => f.Instance == filter);
        }

        /// <summary>
        /// Gets an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator object that can be used to iterate through the collection.</returns>
        public IEnumerator<FilterInfo> GetEnumerator()
        {
            return this.filters.GetEnumerator();
        }

        /// <summary>
        /// Gets an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Removes the specified item from the collection.
        /// </summary>
        /// <param name="filter">The item to remove in the collection.</param>
        public void Remove(IFilter filter)
        {
            this.filters.RemoveAll(f => f.Instance == filter);
        }

        private void AddInternal(FilterInfo filter)
        {
            this.filters.Add(filter);
        }
    }
}