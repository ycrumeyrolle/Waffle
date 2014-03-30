namespace Waffle.Tests.Filters
{
    using System;
    using Waffle;
    using Xunit;
    using Moq;
    using Waffle.Commands;
    using Waffle.Filters;
    using Waffle.Tests.Commands;

    
    public sealed class HandlerContextFixture : IDisposable
    {
        private readonly ProcessorConfiguration config = new ProcessorConfiguration();

        private readonly Mock<ICommand> command = new Mock<ICommand>();
        
        [Fact]
        public void WhenCreatingInstanceWithDefaultCtorThenPropertiesAreDefined()
        {
            // Arrange
            // Act
            CommandHandlerContext context = new CommandHandlerContext();

            // Assert
            Assert.Null(context.Configuration);
            Assert.Null(context.Request);
            Assert.Null(context.Command);
            Assert.Null(context.Descriptor);
            Assert.Null(context.Request);
            Assert.NotNull(context.Items);
            Assert.Equal(0, context.Items.Count);
        }

        [Fact]
        public void WhenCreatingInstanceWithParameterCtorThenPropertiesAreDefined()
        {
            // Arrange
            CommandHandlerRequest request = new CommandHandlerRequest(this.config, this.command.Object);
            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
         
            // Act
            CommandHandlerContext context = new CommandHandlerContext(request, descriptor);

            // Assert
            Assert.Same(this.config, context.Configuration);
            Assert.Same(request, context.Request);
            Assert.Same(request.Command, context.Command);
            Assert.Same(descriptor, context.Descriptor);
            Assert.NotNull(context.Request);
            Assert.NotNull(context.Items);
            Assert.Equal(0, context.Items.Count);
        }

        public void Dispose()
        {
            this.config.Dispose();
        }
    }
}