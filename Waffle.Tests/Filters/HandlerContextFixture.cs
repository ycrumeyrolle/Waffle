namespace Waffle.Tests.Filters
{
    using System;
    using Waffle;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Waffle.Commands;
    using Waffle.Filters;
    using Waffle.Tests.Commands;

    [TestClass]
    public sealed class HandlerContextFixture : IDisposable
    {
        private readonly ProcessorConfiguration config = new ProcessorConfiguration();

        private readonly Mock<ICommand> command = new Mock<ICommand>();
        
        [TestMethod]
        public void WhenCreatingInstanceWithDefaultCtorThenPropertiesAreDefined()
        {
            // Arrange
            // Act
            CommandHandlerContext context = new CommandHandlerContext();

            // Assert
            Assert.IsNull(context.Configuration);
            Assert.IsNull(context.Request);
            Assert.IsNull(context.Command);
            Assert.IsNull(context.Descriptor);
            Assert.IsNull(context.Request);
            Assert.IsNotNull(context.Items);
            Assert.AreEqual(0, context.Items.Count);
        }

        [TestMethod]
        public void WhenCreatingInstanceWithParameterCtorThenPropertiesAreDefined()
        {
            // Arrange
            CommandHandlerRequest request = new CommandHandlerRequest(this.config, this.command.Object);
            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
         
            // Act
            CommandHandlerContext context = new CommandHandlerContext(request, descriptor);

            // Assert
            Assert.AreSame(this.config, context.Configuration);
            Assert.AreSame(request, context.Request);
            Assert.AreSame(request.Command, context.Command);
            Assert.AreSame(descriptor, context.Descriptor);
            Assert.IsNotNull(context.Request);
            Assert.IsNotNull(context.Items);
            Assert.AreEqual(0, context.Items.Count);
        }

        public void Dispose()
        {
            this.config.Dispose();
        }
    }
}