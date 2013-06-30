namespace Waffle.Tests.Filters
{
    using System;
    using System.Linq;
    using Waffle.Tests.Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Waffle.Dispatcher;
    using Waffle.Filters;
    using Waffle.Interception;

    [TestClass]
    public sealed class HandlerDescriptorFixture : IDisposable
    {
        private readonly ProcessorConfiguration config = new ProcessorConfiguration();

        [TestMethod]
        public void WhenGettingFiltersThenReturnsValues()
        {
            // Arrange
            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleHandler));

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
            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleHandler));

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
            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleHandler));
            Mock<Handler> expectedHandler = new Mock<Handler>();
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

        [TestMethod]
        public void Ctor_HandlerLifetime()
        {
            // Arrange
            Mock<IHandlerActivator> activator = new Mock<IHandlerActivator>(MockBehavior.Strict);
            this.config.Services.Replace(typeof(IHandlerActivator), activator.Object);

            // Act
            HandlerDescriptor defaultDescriptor = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleHandler));
            HandlerDescriptor transcientDescriptor = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(TranscientHandler));
            HandlerDescriptor perRequestDescriptor = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(PerRequestHandler));
            HandlerDescriptor perProcessorDescriptor = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(PerProcessorHandler));

            // Assert
            Assert.AreEqual(HandlerLifetime.PerRequest, defaultDescriptor.Lifetime);
            Assert.AreEqual(HandlerLifetime.Transcient, transcientDescriptor.Lifetime);
            Assert.AreEqual(HandlerLifetime.PerRequest, perRequestDescriptor.Lifetime);
            Assert.AreEqual(HandlerLifetime.Processor, perProcessorDescriptor.Lifetime);
        }

        [SimpleExceptionFilter("filter1")]
        [SimpleHandlerFilter]
        [SimpleHandlerFilter]
        [SimpleHandlerFilter]
        [HandlerConfiguration]
        [FakeHandlerConfiguration]
        private class SimpleHandler : Handler<SimpleCommand>
        {
            public override void Handle(SimpleCommand command)
            {
                throw new NotImplementedException();
            }
        }

        [HandlerLifetime(HandlerLifetime.Transcient)]
        private class TranscientHandler : Handler<SimpleCommand>
        {
            public override void Handle(SimpleCommand command)
            {
                throw new NotImplementedException();
            }
        }

        [HandlerLifetime(HandlerLifetime.PerRequest)]
        private class PerRequestHandler : Handler<SimpleCommand>
        {
            public override void Handle(SimpleCommand command)
            {
                throw new NotImplementedException();
            }
        }

        [HandlerLifetime(HandlerLifetime.Processor)]
        private class PerProcessorHandler : Handler<SimpleCommand>
        {
            public override void Handle(SimpleCommand command)
            {
                throw new NotImplementedException();
            }
        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
        public class SimpleHandlerFilter : HandlerFilterAttribute
        {
            public override bool AllowMultiple
            {
                get
                {
                    return false;
                }
            }
        }

        public class FakeHandlerConfigurationAttribute : Attribute, IHandlerConfiguration
        {
            public void Initialize(HandlerSettings settings, HandlerDescriptor descriptor)
            {
            }
        }

        public class HandlerConfigurationAttribute : Attribute, IHandlerConfiguration
        {
            public void Initialize(HandlerSettings settings, HandlerDescriptor descriptor)
            {
                settings.Services.Replace(typeof(IProxyBuilder), new Mock<IProxyBuilder>(MockBehavior.Loose).Object);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.config.Dispose();
        }
    }
}