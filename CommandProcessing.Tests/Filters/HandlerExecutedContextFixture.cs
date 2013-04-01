namespace CommandProcessing.Tests.Filters
{
    using System;

    using CommandProcessing;
    using CommandProcessing.Filters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class HandlerExecutedContextFixture
    {
        [TestMethod]
        public void WhenCreatingInstanceThenPropertiesAreDefined()
        {
            // Arrange
            HandlerContext preContext = new HandlerContext();
            Exception exception = new Exception();

            // Act
            HandlerExecutedContext context = new HandlerExecutedContext(preContext, exception);

            // Assert
            Assert.IsNull(context.Result);
            Assert.IsNotNull(context.Exception);
            Assert.AreSame(context.Exception, exception);
        }

        [TestMethod]
        public void WhenSettingResultThenResultIsDefined()
        {
            // Arrange
            HandlerContext preContext = new HandlerContext();
            Exception exception = new Exception();
            HandlerExecutedContext context = new HandlerExecutedContext(preContext, exception);
            var value = "test";

            // Act
            context.Result = "test";

            // Assert
            Assert.AreEqual(context.Result, value);
        }
    }
}