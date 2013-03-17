namespace CommandProcessing.Tests.Filters
{
    using System;

    using CommandProcessing;
    using CommandProcessing.Filters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ExceptionContextFixture
    {
        [TestMethod]
        public void WhenCreatingInstanceThenPropertiesAreDefined()
        {
            // Arrange
            HandlerContext preContext = new HandlerContext();
            Exception exception = new Exception();

            // Act
            ExceptionContext context = new ExceptionContext(preContext, exception);

            // Assert
            Assert.IsNull(context.Result);
            Assert.IsNotNull(context.Exception);
            Assert.AreSame(context.Exception, exception);
            Assert.IsFalse(context.ExceptionHandled);
        }

        [TestMethod]
        public void WhenSettingResultThenResultIsDefined()
        {
            // Arrange
            HandlerContext preContext = new HandlerContext();
            Exception exception = new Exception();
            ExceptionContext context = new ExceptionContext(preContext, exception);
            var value = "test";

            // Act
            context.Result = "test";

            // Assert
            Assert.AreEqual(context.Result, value);
        }
    }
}