namespace Waffle.Tests.Filters
{
    using System;
    using System.Runtime.ExceptionServices;
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
            ExceptionDispatchInfo exceptionInfo = ExceptionDispatchInfo.Capture(exception);

            // Act
            CommandHandlerExecutedContext context = new CommandHandlerExecutedContext(preContext, exceptionInfo);

            // Assert
            Assert.IsNull(context.Response);
            Assert.IsNotNull(context.ExceptionInfo.SourceException);
            Assert.AreSame(context.ExceptionInfo.SourceException, exception);
        }

        [TestMethod]
        public void WhenSettingResultThenResultIsDefined()
        {
            // Arrange
            CommandHandlerContext preContext = new CommandHandlerContext();
            Exception exception = new Exception();
            ExceptionDispatchInfo exceptionInfo = ExceptionDispatchInfo.Capture(exception);
            CommandHandlerExecutedContext context = new CommandHandlerExecutedContext(preContext, exceptionInfo);
            var value = "test";

            // Act
            context.Response = context.Request.CreateResponse("test");

            // Assert
            Assert.AreEqual(context.Response.Value, value);
        }
    }
}