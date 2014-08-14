namespace Waffle.Tests.Filters
{
    using Xunit;
    using Moq;
    using Waffle.Filters;
    using Waffle.Tests.Helpers;
    
    public class FilterInfoTests
    {
        [Fact]
        public void WhenCreatingFilterInfoThenPropertiesAreDefined()
        {
            // Arrange
            IFilter filter1 = new Mock<ICommandHandlerFilter>().Object;
            IFilter filter2 = new Mock<ICommandHandlerFilter>().Object;

            // Act
            FilterInfo filterInfo1 = new FilterInfo(filter1, FilterScope.Global);
            FilterInfo filterInfo2 = new FilterInfo(filter2, FilterScope.Handler);

            // Assert
            Assert.NotNull(filterInfo1);
            Assert.Equal(FilterScope.Global, filterInfo1.Scope);
            Assert.Same(filter1, filterInfo1.Instance);

            Assert.NotNull(filterInfo2);
            Assert.Equal(FilterScope.Handler, filterInfo2.Scope);
            Assert.Same(filter2, filterInfo2.Instance);
        }

        [Fact]
        public void WhenCreatingFilterInfoWithNullParameterThenThrowsException()
        {   
            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => new FilterInfo(null, FilterScope.Global), "instance");
        }
    }
}