namespace CommandProcessing.Tests.Filters
{
    using System;
    using System.Linq;
    using CommandProcessing.Filters;
    using CommandProcessing.Tests.Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class FilterGroupingFixture
    {
        [TestMethod]
        public void WhenGroupingFiltersThenFiltersAreCategorized()
        {
            // Arrange
            var group = CreateTestableFilterGrouping();

            // Act
            var exceptionFilters = group.ExceptionFilters;
            var handlerFilters = group.HandlerFilters;

            // Assert
            Assert.IsNotNull(exceptionFilters);
            Assert.AreEqual(5, exceptionFilters.Count());

            Assert.IsNotNull(handlerFilters);
            Assert.AreEqual(4, handlerFilters.Count());
        }

        [TestMethod]
        public void WhenGettingWithNullParameterThenThrowsException()
        {   
            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => new FilterGrouping(null), "filters");
        }

        private static FilterGrouping CreateTestableFilterGrouping()
        {
            var filters = new[]
                {
                    new FilterInfo(new Mock<IHandlerFilter>().Object, FilterScope.Global),
                    new FilterInfo(new Mock<ExceptionFilterAttribute>().Object, FilterScope.Global),
                    new FilterInfo(new Mock<IFilter>().Object, FilterScope.Global),
                    new FilterInfo(new Mock<IHandlerFilter>().Object, FilterScope.Global),
                    new FilterInfo(new Mock<ExceptionFilterAttribute>().Object, FilterScope.Global),
                    new FilterInfo(new Mock<IHandlerFilter>().Object, FilterScope.Global),
                    new FilterInfo(new Mock<IFilter>().Object, FilterScope.Global),
                    new FilterInfo(new Mock<ExceptionFilterAttribute>().Object, FilterScope.Global),
                    new FilterInfo(new Mock<IHandlerFilter>().Object, FilterScope.Global),
                    new FilterInfo(new Mock<ExceptionFilterAttribute>().Object, FilterScope.Global),
                    new FilterInfo(new Mock<ExceptionFilterAttribute>().Object, FilterScope.Global)
                };
            FilterGrouping group = new FilterGrouping(filters);
            return group;
        }
    }
}