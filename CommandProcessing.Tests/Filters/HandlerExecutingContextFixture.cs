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
            Assert.IsNull(context.Result);
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
            context.Result = "test";

            // Assert
            Assert.AreEqual(context.Result, value);
        }
    }
}