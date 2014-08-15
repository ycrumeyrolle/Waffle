namespace Waffle.Tests.Filters
{
    using System;
    using System.Runtime.ExceptionServices;
    using System.Threading;
    using Moq;
    using Waffle.Filters;
    using Waffle.Tests.Helpers;
    using Xunit;
    
    public class ExceptionFilterAttributeTests
    {
        [Fact]
        public void WhenExecuteExceptionFilterAsyncThenReturnsCompletedTask()
        {
            // Arrange
            Mock<MessageProcessorTests.ISpy> spy = new Mock<MessageProcessorTests.ISpy>();
            IExceptionFilter filter = new CustomExceptionFilterAttribute(spy.Object);
            CommandHandlerContext handlerContext = new CommandHandlerContext();
            Exception exception = new Exception();
            ExceptionDispatchInfo exceptionInfo = ExceptionDispatchInfo.Capture(exception);
            CommandHandlerExecutedContext handlerExecutedContext = new CommandHandlerExecutedContext(handlerContext, exceptionInfo);
            CancellationToken cancellationToken = new CancellationToken();

            // Act
            var task = filter.ExecuteExceptionFilterAsync(handlerExecutedContext, cancellationToken);

            // Assert
            Assert.NotNull(task);
            Assert.True(task.IsCompleted);
            spy.Verify(s => s.Spy("OnException"), Times.Once());
        }

        [Fact]
        public void WhenExecuteExceptionFilterAsyncWithoutContextThenThrowsArgumentNullException()
        {
            // Arrange
            Mock<MessageProcessorTests.ISpy> spy = new Mock<MessageProcessorTests.ISpy>();
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
            private readonly MessageProcessorTests.ISpy spy;

            public CustomExceptionFilterAttribute(MessageProcessorTests.ISpy spy)
            {
                this.spy = spy;
            }

            public override void OnException(CommandHandlerExecutedContext handlerExecutedContext)
            {
                this.spy.Spy("OnException");
                base.OnException(handlerExecutedContext);
            }
        }
    }
}