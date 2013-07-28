namespace Waffle.Tests.Filters
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Waffle.Commands;
    using Waffle.Filters;
    using Waffle.Interception;
    using Waffle.Tests.Commands;

    [TestClass]
    public sealed class HandlerDescriptorFixture : IDisposable
    {
        private readonly ProcessorConfiguration config = new ProcessorConfiguration();

        [TestMethod]
        public void WhenGettingFiltersThenReturnsValues()
        {
            // Arrange
            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));

            // Act
            var filters = descriptor.GetFilters();

            // Assert
            Assert.IsNotNull(filters);
            Assert.AreEqual(4, filters.Count());
            Assert.AreEqual(1, filters.OfType<IExceptionFilter>().Count());
            Assert.AreEqual(3, filters.OfType<ICommandHandlerFilter>().Count());
        }

        [TestMethod]
        public void WhenGettingFilterPipelineThenReturnsDistinctValues()
        {
            // Arrange
            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));

            // Act
            var filters = descriptor.GetFilterPipeline();

            // Assert
            Assert.IsNotNull(filters);
            Assert.AreEqual(2, filters.Count());
            Assert.AreEqual(1, filters.Select(f => f.Instance).OfType<IExceptionFilter>().Count());
            Assert.AreEqual(1, filters.Select(f => f.Instance).OfType<ICommandHandlerFilter>().Count());
        }

        [TestMethod]
        public void WhenCreatingHandlerThenDelegateToActivator()
        {
            // Arrange
            Mock<ICommandHandlerActivator> activator = new Mock<ICommandHandlerActivator>(MockBehavior.Strict);
            this.config.Services.Replace(typeof(ICommandHandlerActivator), activator.Object);
            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            Mock<CommandHandler> expectedHandler = new Mock<CommandHandler>();
            activator
                .Setup(a => a.Create(It.IsAny<CommandHandlerRequest>(), It.IsAny<CommandHandlerDescriptor>()))
                .Returns(expectedHandler.Object);
            CommandHandlerRequest request = new CommandHandlerRequest(new ProcessorConfiguration(), new Mock<ICommand>().Object);

            // Act
            var handler = descriptor.CreateHandler(request);

            // Assert
            Assert.IsNotNull(handler);
            Assert.AreSame(expectedHandler.Object, handler);
            activator.Verify(a => a.Create(It.IsAny<CommandHandlerRequest>(), It.IsAny<CommandHandlerDescriptor>()), Times.Once());
        }

        [TestMethod]
        public void Ctor_HandlerLifetime()
        {
            // Arrange
            Mock<ICommandHandlerActivator> activator = new Mock<ICommandHandlerActivator>(MockBehavior.Strict);
            this.config.Services.Replace(typeof(ICommandHandlerActivator), activator.Object);

            // Act
            CommandHandlerDescriptor defaultDescriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            CommandHandlerDescriptor transcientDescriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(TranscientCommandHandler));
            CommandHandlerDescriptor perRequestDescriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(PerRequestCommandHandler));
            CommandHandlerDescriptor perProcessorDescriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(PerProcessorCommandHandler));

            // Assert
            Assert.AreEqual(HandlerLifetime.PerRequest, defaultDescriptor.Lifetime);
            Assert.AreEqual(HandlerLifetime.Transient, transcientDescriptor.Lifetime);
            Assert.AreEqual(HandlerLifetime.PerRequest, perRequestDescriptor.Lifetime);
            Assert.AreEqual(HandlerLifetime.Processor, perProcessorDescriptor.Lifetime);
        }

        [SimpleExceptionFilter("filter1")]
        [SimpleCommandHandlerFilter]
        [SimpleCommandHandlerFilter]
        [SimpleCommandHandlerFilter]
        [HandlerConfiguration]
        [FakeHandlerConfiguration]
        private class SimpleCommandHandler : CommandHandler<SimpleCommand>
        {
            public override void Handle(SimpleCommand command, CommandHandlerContext context)
            {
                throw new NotImplementedException();
            }
        }

        [HandlerLifetime(HandlerLifetime.Transient)]
        private class TranscientCommandHandler : CommandHandler<SimpleCommand>
        {
            public override void Handle(SimpleCommand command, CommandHandlerContext context)
            {
                throw new NotImplementedException();
            }
        }

        [HandlerLifetime(HandlerLifetime.PerRequest)]
        private class PerRequestCommandHandler : CommandHandler<SimpleCommand>
        {
            public override void Handle(SimpleCommand command, CommandHandlerContext context)
            {
                throw new NotImplementedException();
            }
        }

        [HandlerLifetime(HandlerLifetime.Processor)]
        private class PerProcessorCommandHandler : CommandHandler<SimpleCommand>
        {
            public override void Handle(SimpleCommand command, CommandHandlerContext context)
            {
                throw new NotImplementedException();
            }
        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
        public class SimpleCommandHandlerFilter : CommandHandlerFilterAttribute
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
            public void Initialize(CommandHandlerSettings settings, CommandHandlerDescriptor descriptor)
            {
            }
        }

        public class HandlerConfigurationAttribute : Attribute, IHandlerConfiguration
        {
            public void Initialize(CommandHandlerSettings settings, CommandHandlerDescriptor descriptor)
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