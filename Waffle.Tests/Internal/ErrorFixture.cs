namespace Waffle.Tests.Internal
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Waffle.Internal;

    [TestClass]
    public class ErrorTest
    {
        [TestMethod]
        public void Format()
        {
            // Arrange
            string expected = "The formatted message";

            // Act
            string actual = Error.Format("The {0} message", "formatted");

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}
