namespace CommandProcessing.Tests
{
    using CommandProcessing;
    using CommandProcessing.Dispatcher;
    using CommandProcessing.Filters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CommandProcessorFixture
    {
        [TestMethod]
        public void WhenCreatingProcessorWithoutConfigThenHasDefaultConfig()
        {
            // Act
            CommandProcessor processor = new CommandProcessor();

            // Assert
            Assert.IsNotNull(processor.Configuration);
        }

        [TestMethod]
        public void WhenCreatingProcessorWithCustomConfigThenHasCustomConfig()
        {
            // Assign
            using (ProcessorConfiguration config = new ProcessorConfiguration())
            {
                // Act
                CommandProcessor processor = new CommandProcessor(config);

                // Assert
                Assert.IsNotNull(processor.Configuration);
                Assert.AreSame(config, processor.Configuration);
            }
        }

        [TestMethod]
        public void WhenProcessingSimpleCommandThenCommandIsProcessed()
        {
            // Assign
            Mock<ICommandHandler<SimpleCommand>> handler = new Mock<ICommandHandler<SimpleCommand>>(MockBehavior.Strict);
            Mock<IHandlerActivator> activator = new Mock<IHandlerActivator>(MockBehavior.Strict);
            CommandProcessor processor = new CommandProcessor();
            processor.Configuration.Services.Replace(typeof(IHandlerActivator), activator.Object);
            SimpleCommand command = new SimpleCommand();
            handler.Setup(h => h.Handle(It.IsAny<ICommand>())).Returns((EmptyResult)null);
            activator
                .Setup(a => a.Create(It.IsAny<HandlerRequest>(), It.IsAny<HandlerDescriptor>()))
                .Returns(handler.Object);
            
            // Act
            processor.Process(command);

            // Assert
            handler.Verify(h => h.Handle(It.IsAny<ICommand>()), Times.Once());
        }
    }
}