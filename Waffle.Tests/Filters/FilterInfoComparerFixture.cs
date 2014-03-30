namespace Waffle.Tests.Filters
{
    using Moq;
    using System;
    using System.Collections.Generic;
    using Waffle.Filters;
    using Xunit;
    using Xunit.Extensions;

    public class FilterInfoComparerFixture
    {
        public static IEnumerable<object[]> FilterData
        {
            get
            {
                IFilter f = new Mock<IFilter>().Object;

                yield return new object[] { null, null, 0 };
                yield return new object[] { new FilterInfo(f, FilterScope.Handler), null, 1 };
                yield return new object[] { null, new FilterInfo(f, FilterScope.Handler), -1 };
                yield return new object[] { new FilterInfo(f, FilterScope.Handler), new FilterInfo(f, FilterScope.Handler), 0 };
                yield return new object[] { new FilterInfo(f, FilterScope.Global), new FilterInfo(f, FilterScope.Handler), -1 };
                yield return new object[] { new FilterInfo(f, FilterScope.Handler), new FilterInfo(f, FilterScope.Global), 1 };
            }
        }

        [Theory]
        [PropertyData("FilterData")]
        public void Compare(FilterInfo x, FilterInfo y, int expectedSign)
        {
            int result = FilterInfoComparer.Instance.Compare(x, y);

            Assert.Equal(expectedSign, Math.Sign(result));
        }
    }
}