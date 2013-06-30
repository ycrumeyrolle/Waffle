namespace Waffle.Tests.Eventing
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Waffle.Eventing;
    using Waffle.Filters;
    using Waffle.Tests.Helpers;

    [TestClass]
    public class EventAttributeFixture
    {
        [TestMethod]
        public void WhenCreatingAttributeThenPropertiesAreDefined()
        {
            // Arrange & Act
            EventAttribute attribute = new EventAttribute("test");

            // Assert
            Assert.AreEqual("test", attribute.EventName);
        }

        [TestMethod]
        public void WhenHandlerExecutedThenMessageIsPublished()
        {
            // Arrange
            Mock<IMessageHub> messageHub = new Mock<IMessageHub>(MockBehavior.Strict);
            HandlerRequest request = new HandlerRequest(new ProcessorConfiguration(), new Mock<ICommand>().Object);
            HandlerContext context = new HandlerContext(request, null);
            context.Configuration.Services.Replace(typeof(IMessageHub), messageHub.Object);
            HandlerExecutedContext executedContext = new HandlerExecutedContext(context, null);
            messageHub.Setup(h => h.Publish("test", It.IsAny<object>()));

            EventAttribute attribute = new EventAttribute("test");

            // Act
            attribute.OnCommandExecuted(executedContext);

            // Assert
            messageHub.Verify(h => h.Publish("test", It.IsAny<object>()), Times.Once());
        }

        [TestMethod]
        public void WhenHandlerExecutedWithoutContextThenThrowsArgumentNullException()
        {
            // Arrange
            EventAttribute attribute = new EventAttribute("test");

            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => attribute.OnCommandExecuted(null), "handlerExecutedContext");
        }
    }
}
