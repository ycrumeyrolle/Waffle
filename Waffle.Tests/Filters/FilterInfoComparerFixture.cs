namespace Waffle.Tests.Filters
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Waffle.Filters;

    [TestClass]
    public class FilterInfoComparerFixture
    {
        [TestMethod]
        public void Compare()
        {
            IFilter f = new Mock<IFilter>().Object;
            var values = new Tuple<FilterInfo, FilterInfo, int>[] 
            {
                    Tuple.Create<FilterInfo, FilterInfo, int>(null, null, 0),
                    Tuple.Create<FilterInfo, FilterInfo, int>(new FilterInfo(f, FilterScope.Handler), null, 1),
                    Tuple.Create<FilterInfo, FilterInfo, int>(null, new FilterInfo(f, FilterScope.Handler), -1),
                    Tuple.Create<FilterInfo, FilterInfo, int>(new FilterInfo(f, FilterScope.Handler), new FilterInfo(f, FilterScope.Handler), 0),
                    Tuple.Create<FilterInfo, FilterInfo, int>(new FilterInfo(f, FilterScope.Global), new FilterInfo(f, FilterScope.Handler), -1),
                    Tuple.Create<FilterInfo, FilterInfo, int>(new FilterInfo(f, FilterScope.Handler), new FilterInfo(f, FilterScope.Global), 1)
            };
            foreach (var tuple in values)
            {
                this.Compare(tuple.Item1, tuple.Item2, tuple.Item3);
            }
        }

        public void Compare(FilterInfo x, FilterInfo y, int expectedSign)
        {
            int result = FilterInfoComparer.Instance.Compare(x, y);

            Assert.AreEqual(expectedSign, Math.Sign(result));
        }
    }
}