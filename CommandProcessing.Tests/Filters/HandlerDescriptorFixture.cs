namespace CommandProcessing.Tests.Filters
{
    using System;
    using System.Linq;
    using CommandProcessing.Dispatcher;
    using CommandProcessing.Filters;
    using CommandProcessing.Tests.Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class HandlerDescriptorFixture
    {
        private readonly ProcessorConfiguration config = new ProcessorConfiguration();

        [TestMethod]
        public void WhenGettingFiltersThenReturnsValues()
        {
            // Arrange
            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleHandler));

            // Act
            var filters = descriptor.GetFilters();

            // Assert
            Assert.IsNotNull(filters);
            Assert.AreEqual(4, filters.Count());
            Assert.AreEqual(1, filters.OfType<IExceptionFilter>().Count());
            Assert.AreEqual(3, filters.OfType<IHandlerFilter>().Count());
        }

        [TestMethod]
        public void WhenGettingFilterPipelineThenReturnsDistinctValues()
        {
            // Arrange
            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleHandler));

            // Act
            var filters = descriptor.GetFilterPipeline();

            // Assert
            Assert.IsNotNull(filters);
            Assert.AreEqual(2, filters.Count());
            Assert.AreEqual(1, filters.Select(f => f.Instance).OfType<IExceptionFilter>().Count());
            Assert.AreEqual(1, filters.Select(f => f.Instance).OfType<IHandlerFilter>().Count());
        }

        [TestMethod]
        public void WhenCreatingHandlerThenDelegateToActivator()
        {
            // Arrange
            Mock<IHandlerActivator> activator = new Mock<IHandlerActivator>(MockBehavior.Strict);
            this.config.Services.Replace(typeof(IHandlerActivator), activator.Object);
            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleHandler));
            Mock<ICommandHandler> expectedHandler = new Mock<ICommandHandler>();
            activator
                .Setup(a => a.Create(It.IsAny<HandlerRequest>(), It.IsAny<HandlerDescriptor>()))
                .Returns(expectedHandler.Object);
            HandlerRequest request = new HandlerRequest(new ProcessorConfiguration(), new Mock<ICommand>().Object);

            // Act
            var handler = descriptor.CreateHandler(request);

            // Assert
            Assert.IsNotNull(handler);
            Assert.AreSame(expectedHandler.Object, handler);
            activator.Verify(a => a.Create(It.IsAny<HandlerRequest>(), It.IsAny<HandlerDescriptor>()), Times.Once());
        }

        [SimpleExceptionFilter("filter1")]
        [SimpleHandlerFilter]
        [SimpleHandlerFilter]
        [SimpleHandlerFilter]
        private class SimpleHandler : Handler<SimpleCommand>
        {
            public override void Handle(SimpleCommand command)
            {
                throw new NotImplementedException();
            }
        }
        
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
        public class SimpleHandlerFilter : FilterAttribute, IHandlerFilter
        {
            public void OnCommandExecuting(HandlerExecutingContext context)
            {
                throw new NotImplementedException();
            }

            public void OnCommandExecuted(HandlerExecutedContext context)
            {
                throw new NotImplementedException();
            }

            public override bool AllowMultiple
            {
                get
                {
                    return false;
                }
            }
        }
    }
}