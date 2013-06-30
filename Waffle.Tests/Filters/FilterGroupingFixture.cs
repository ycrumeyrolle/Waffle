namespace Waffle.Tests.Filters
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Waffle.Filters;
    using Waffle.Tests.Helpers;

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
            Assert.AreEqual(5, exceptionFilters.Length);

            Assert.IsNotNull(handlerFilters);
            Assert.AreEqual(4, handlerFilters.Length);
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