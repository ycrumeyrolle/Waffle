namespace CommandProcessing.Tests.Eventing
{
    using CommandProcessing.Eventing;
    using CommandProcessing.Filters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

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
    }
}
