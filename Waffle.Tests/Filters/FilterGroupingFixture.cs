namespace Waffle.Tests.Filters
{
    using Xunit;
    using Moq;
    using Waffle.Filters;
    using Waffle.Tests.Helpers;

    
    public class FilterGroupingFixture
    {
        [Fact]
        public void WhenGroupingFiltersThenFiltersAreCategorized()
        {
            // Arrange
            var group = CreateTestableFilterGrouping();

            // Act
            var exceptionFilters = group.ExceptionFilters;
            var handlerFilters = group.CommandHandlerFilters;

            // Assert
            Assert.NotNull(exceptionFilters);
            Assert.Equal(5, exceptionFilters.Length);

            Assert.NotNull(handlerFilters);
            Assert.Equal(4, handlerFilters.Length);
        }

        [Fact]
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