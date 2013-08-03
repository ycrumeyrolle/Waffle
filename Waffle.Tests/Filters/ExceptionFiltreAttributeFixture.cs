namespace Waffle.Tests.Filters
{
    using System;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Waffle.Filters;
    using Waffle.Tests.Helpers;

    [TestClass]
    public class ExceptionFiltreAttributeFixture
    {
        [TestMethod]
        public void WhenExecuteExceptionFilterAsyncThenReturnsCompletedTask()
        {
            // Arrange
            Mock<MessageProcessorFixture.ISpy> spy = new Mock<MessageProcessorFixture.ISpy>();
            IExceptionFilter filter = new CustomExceptionFilterAttribute(spy.Object);
            CommandHandlerContext handlerContext = new CommandHandlerContext();
            Exception exception = new Exception();
            HandlerExecutedContext handlerExecutedContext = new HandlerExecutedContext(handlerContext, exception);
            CancellationToken cancellationToken = new CancellationToken();

            // Act
            var task = filter.ExecuteExceptionFilterAsync(handlerExecutedContext, cancellationToken);

            // Assert
            Assert.IsNotNull(task);
            Assert.IsTrue(task.IsCompleted);
            spy.Verify(s => s.Spy("OnException"), Times.Once());
        }

        [TestMethod]
        public void WhenExecuteExceptionFilterAsyncWithoutContextThenThrowsArgumentNullException()
        {
            // Arrange
            Mock<MessageProcessorFixture.ISpy> spy = new Mock<MessageProcessorFixture.ISpy>();
            IExceptionFilter filter = new CustomExceptionFilterAttribute(spy.Object);
        
            CancellationToken cancellationToken = new CancellationToken();

            // Act
            ExceptionAssert.ThrowsArgumentNull(() => filter.ExecuteExceptionFilterAsync(null, cancellationToken), "handlerExecutedContext");

            // Assert
            spy.Verify(s => s.Spy("OnException"), Times.Never());
        }

        [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
        private class CustomExceptionFilterAttribute : ExceptionFilterAttribute
        {
            private readonly MessageProcessorFixture.ISpy spy;

            public CustomExceptionFilterAttribute(MessageProcessorFixture.ISpy spy)
            {
                this.spy = spy;
            }

            public override void OnException(HandlerExecutedContext handlerExecutedContext)
            {
                this.spy.Spy("OnException");
                base.OnException(handlerExecutedContext);
            }
        }
    }
}