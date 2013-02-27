namespace CommandProcessing.Filters
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class FilterCollection : IEnumerable<FilterInfo>
    {
        private readonly List<FilterInfo> filters = new List<FilterInfo>();

        public int Count
        {
            get
            {
                return this.filters.Count;
            }
        }

        public void Add(IFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

            this.AddInternal(new FilterInfo(filter, FilterScope.Global));
        }

        public void Clear()
        {
            this.filters.Clear();
        }

        public bool Contains(IFilter filter)
        {
            return this.filters.Any(f => f.Instance == filter);
        }

        public IEnumerator<FilterInfo> GetEnumerator()
        {
            return this.filters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

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