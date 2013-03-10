namespace CommandProcessing.Tests.Filters
{
    using System;
    using CommandProcessing;
    using CommandProcessing.Filters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class HandlerExecutingContextFixture
    {
        [TestMethod]
        public void WhenCreatingInstanceThenPropertiesAreDefined()
        {
            // Arrange
            HandlerContext preContext = new HandlerContext();

            // Act
            HandlerExecutingContext context = new HandlerExecutingContext(preContext);
       
            // Assert
            Assert.IsNotNull(context.Result);
            Assert.IsInstanceOfType(context.Result, typeof(EmptyResult));
            Assert.IsNotNull(context.CommandContext);
            Assert.AreSame(context.CommandContext, preContext);
        }

        [TestMethod]
        public void WhenSettingResultThenResultIsDefined()
        {
            // Arrange
            HandlerContext preContext = new HandlerContext();
            Exception exception = new Exception();
            HandlerExecutingContext context = new HandlerExecutingContext(preContext);
            var value = "test";

            // Act
            context.Result = new HandlerResult("test");

            // Assert
            Assert.IsNotNull(context.Result);
            Assert.AreEqual(context.Result.Value, value);
        }
    }
}