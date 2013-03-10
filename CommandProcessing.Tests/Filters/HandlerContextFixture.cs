namespace CommandProcessing.Tests.Filters
{
    using System;

    using CommandProcessing;
    using CommandProcessing.Filters;
    using CommandProcessing.Tests.Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class HandlerContextFixture
    {
        private readonly ProcessorConfiguration config = new ProcessorConfiguration();

        private readonly Mock<ICommand> command = new Mock<ICommand>();
        
        [TestMethod]
        public void WhenCreatingInstanceWithDefaultCtorThenPropertiesAreDefined()
        {
            // Arrange
            // Act
            HandlerContext context = new HandlerContext();

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
            HandlerRequest request = new HandlerRequest(this.config, this.command.Object);
            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleHandler));

            // Act
            HandlerContext context = new HandlerContext(request, descriptor);

            // Assert
            Assert.AreSame(this.config, context.Configuration);
            Assert.AreSame(request, context.Request);
            Assert.AreSame(request.Command, context.Command);
            Assert.AreSame(descriptor, context.Descriptor);
            Assert.IsNotNull(context.Request);
            Assert.IsNotNull(context.Items);
            Assert.AreEqual(0, context.Items.Count);
        }

        [TestMethod]
        public void WhenCreatingInstanceWithCopyCtorThenPropertiesAreDefined()
        {
            // Arrange
            HandlerRequest request = new HandlerRequest(this.config, this.command.Object);
            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleHandler));
            HandlerContext preContext = new HandlerContext(request, descriptor);

            // Act
            HandlerContext context = new SubHandlerContext(preContext);

            // Assert
            Assert.AreSame(this.config, context.Configuration);
            Assert.AreSame(request, context.Request);
            Assert.AreSame(request.Command, context.Command);
            Assert.AreSame(descriptor, context.Descriptor);
            Assert.IsNotNull(context.Request);
            Assert.IsNotNull(context.Items);
            Assert.AreEqual(0, context.Items.Count);
        }

        [TestMethod]
        public void WhenCreatingFilterInfoWithNullParameterThenThrowsException()
        {
            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => new SubHandlerContext(null), "context");
        }

        private class SubHandlerContext : HandlerContext
        {
            public SubHandlerContext(HandlerContext context)
                : base(context)
            {
            }
        }
    }
}