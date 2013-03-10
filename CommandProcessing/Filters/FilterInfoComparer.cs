namespace CommandProcessing.Filters
{
    using System.Collections.Generic;

    internal sealed class FilterInfoComparer : IComparer<FilterInfo>
    {
        private static readonly FilterInfoComparer Singleton = new FilterInfoComparer();

        public static FilterInfoComparer Instance
        {
            get
            {
                return FilterInfoComparer.Singleton;
            }
        }

        public int Compare(FilterInfo x, FilterInfo y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null)
            {
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            return x.Scope - y.Scope;
        }
    }
}