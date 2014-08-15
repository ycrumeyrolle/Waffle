namespace Waffle.Tests.Filters
{
    using System;
    using Waffle.Filters;
    using Xunit;
    
    public class FilterAttributeTests
    {
        [Fact]
        public void WhenMultipleIsAllowedThenReturnsTrue()
        {
            // Arrange
            FilterAttribute filter = new AllowMultipleFilterAttribute();

            // Act
            var allow = filter.AllowMultiple;

            // Assert
            Assert.True(allow);
        }

        [Fact]
        public void WhenMultipleIsDisallowedThenReturnsFalse()
        {
            // Arrange
            FilterAttribute filter = new DisallowMultipleFilterAttribute();

            // Act
            var allow = filter.AllowMultiple;

            // Assert
            Assert.False(allow);
        }

        [Fact]
        public void WhenMultipleIsUndefinedThenReturnsTrue()
        {
            // Arrange
            FilterAttribute filter = new UndefindMultipleFilterAttribute();

            // Act
            var allow = filter.AllowMultiple;

            // Assert
            Assert.True(allow);
        }

        [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
        private class AllowMultipleFilterAttribute : FilterAttribute
        {
        }

        [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
        private class DisallowMultipleFilterAttribute : FilterAttribute
        {
        }

        private class UndefindMultipleFilterAttribute : FilterAttribute
        {
        }
    }
}