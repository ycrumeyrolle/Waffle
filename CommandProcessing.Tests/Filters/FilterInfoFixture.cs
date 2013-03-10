namespace CommandProcessing.Tests.Filters
{
    using System.Linq;
    using CommandProcessing.Filters;
    using CommandProcessing.Tests.Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class FilterInfoFixture
    {
        [TestMethod]
        public void WhenCreatingFilterInfoThenPropertiesAreDefined()
        {
            // Arrange
            IFilter filter1 = new Mock<IHandlerFilter>().Object;
            IFilter filter2 = new Mock<IHandlerFilter>().Object;

            // Act
            FilterInfo filterInfo1 = new FilterInfo(filter1, FilterScope.Global);
            FilterInfo filterInfo2 = new FilterInfo(filter2, FilterScope.Handler);

            // Assert
            Assert.IsNotNull(filterInfo1);
            Assert.AreEqual(FilterScope.Global, filterInfo1.Scope);
            Assert.AreSame(filter1, filterInfo1.Instance);

            Assert.IsNotNull(filterInfo2);
            Assert.AreEqual(FilterScope.Handler, filterInfo2.Scope);
            Assert.AreSame(filter2, filterInfo2.Instance);
        }

        [TestMethod]
        public void WhenCreatingFilterInfoWithNullParameterThenThrowsException()
        {   
            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => new FilterInfo(null, FilterScope.Global), "instance");
        }
    }
}