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
            var handlerFilters = group.CommandHandlerFilters;

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
            ExceptionAssert.ThrowsArgumentNull(() => new CommandFilterGrouping(null), "filters");
        }

        private static CommandFilterGrouping CreateTestableFilterGrouping()
        {
            var filters = new[]
                {
                    new FilterInfo(new Mock<ICommandHandlerFilter>().Object, FilterScope.Global),
                    new FilterInfo(new Mock<ExceptionFilterAttribute>().Object, FilterScope.Global),
                    new FilterInfo(new Mock<IFilter>().Object, FilterScope.Global),
                    new FilterInfo(new Mock<ICommandHandlerFilter>().Object, FilterScope.Global),
                    new FilterInfo(new Mock<ExceptionFilterAttribute>().Object, FilterScope.Global),
                    new FilterInfo(new Mock<ICommandHandlerFilter>().Object, FilterScope.Global),
                    new FilterInfo(new Mock<IFilter>().Object, FilterScope.Global),
                    new FilterInfo(new Mock<ExceptionFilterAttribute>().Object, FilterScope.Global),
                    new FilterInfo(new Mock<ICommandHandlerFilter>().Object, FilterScope.Global),
                    new FilterInfo(new Mock<ExceptionFilterAttribute>().Object, FilterScope.Global),
                    new FilterInfo(new Mock<ExceptionFilterAttribute>().Object, FilterScope.Global)
                };
            CommandFilterGrouping group = new CommandFilterGrouping(filters);
            return group;
        }
    }
}