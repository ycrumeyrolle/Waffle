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
            Assert.IsNotNull(context.Result);
            Assert.IsInstanceOfType(context.Result, typeof(EmptyResult));
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
            context.Result = new HandlerResult("test");

            // Assert
            Assert.IsNotNull(context.Result);
            Assert.AreEqual(context.Result.Value, value);
        }
    }
}