namespace Waffle.Tests.Filters
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Waffle.Filters;

    [TestClass]
    public class HandlerExecutedContextFixture
    {
        [TestMethod]
        public void WhenCreatingInstanceThenPropertiesAreDefined()
        {
            // Arrange
            CommandHandlerContext preContext = new CommandHandlerContext();
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
            CommandHandlerContext preContext = new CommandHandlerContext();
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