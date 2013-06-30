namespace Waffle.Tests.Filters
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Waffle.Filters;

    [TestClass]
    public class FilterAttributeFixture
    {
        [TestMethod]
        public void WhenMultipleIsAllowedThenReturnsTrue()
        {
            // Arrange
            FilterAttribute filter = new AllowMultipleFilterAttribute();

            // Act
            var allow = filter.AllowMultiple;

            // Assert
            Assert.IsTrue(allow);
        }

        [TestMethod]
        public void WhenMultipleIsDisallowedThenReturnsFalse()
        {
            // Arrange
            FilterAttribute filter = new DisallowMultipleFilterAttribute();

            // Act
            var allow = filter.AllowMultiple;

            // Assert
            Assert.IsFalse(allow);
        }

        [TestMethod]
        public void WhenMultipleIsUndefinedThenReturnsTrue()
        {
            // Arrange
            FilterAttribute filter = new UndefindMultipleFilterAttribute();

            // Act
            var allow = filter.AllowMultiple;

            // Assert
            Assert.IsTrue(allow);
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