namespace Waffle.Tests.Filters
{
    using System;
    using System.Linq;
    using Moq;
    using Waffle.Commands;
    using Waffle.Filters;
    using Waffle.Interception;
    using Xunit;

    public sealed class HandlerDescriptorTests : IDisposable
    {
        private readonly ProcessorConfiguration config = new ProcessorConfiguration();

        [Fact]
        public void WhenGettingFiltersThenReturnsValues()
        {
            // Arrange
            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));

            // Act
            var filters = descriptor.GetFilters();

            // Assert
            Assert.NotNull(filters);
            Assert.Equal(4, filters.Count());
            Assert.Equal(1, filters.OfType<IExceptionFilter>().Count());
            Assert.Equal(3, filters.OfType<ICommandHandlerFilter>().Count());
        }

        [Fact]
        public void WhenGettingFilterPipelineThenReturnsDistinctValues()
        {
            // Arrange
            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));

            // Act
            var filters = descriptor.GetFilterPipeline();

            // Assert
            Assert.NotNull(filters);
            Assert.Equal(2, filters.Count());
            Assert.Equal(1, filters.Select(f => f.Instance).OfType<IExceptionFilter>().Count());
            Assert.Equal(1, filters.Select(f => f.Instance).OfType<ICommandHandlerFilter>().Count());
        }

        [Fact]
        public void WhenCreatingHandlerThenDelegateToActivator()
        {
            // Arrange
            Mock<ICommandHandlerActivator> activator = new Mock<ICommandHandlerActivator>(MockBehavior.Strict);
            this.config.Services.Replace(typeof(ICommandHandlerActivator), activator.Object);
            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            Mock<MessageHandler> expectedHandler = new Mock<MessageHandler>();
            activator
                .Setup(a => a.Create(It.IsAny<CommandHandlerRequest>(), It.IsAny<CommandHandlerDescriptor>()))
                .Returns(expectedHandler.Object);
            CommandHandlerRequest request = new CommandHandlerRequest(new ProcessorConfiguration(), new Mock<ICommand>().Object);

            // Act
            var handler = descriptor.CreateHandler(request);

            // Assert
            Assert.NotNull(handler);
            Assert.Same(expectedHandler.Object, handler);
            activator.Verify(a => a.Create(It.IsAny<CommandHandlerRequest>(), It.IsAny<CommandHandlerDescriptor>()), Times.Once());
        }

        [Fact]
        public void Ctor_HandlerLifetime()
        {
            // Arrange
            Mock<ICommandHandlerActivator> activator = new Mock<ICommandHandlerActivator>(MockBehavior.Strict);
            this.config.Services.Replace(typeof(ICommandHandlerActivator), activator.Object);

            // Act
            CommandHandlerDescriptor defaultDescriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            CommandHandlerDescriptor transcientDescriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(TranscientCommandHandler));
            CommandHandlerDescriptor perRequestDescriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(PerRequestCommandHandler));
            CommandHandlerDescriptor singletonDescriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SingletonCommandHandler));

            // Assert
            Assert.Equal(this.config.DefaultHandlerLifetime, defaultDescriptor.Lifetime);
            Assert.Equal(HandlerLifetime.Transient, transcientDescriptor.Lifetime);
            Assert.Equal(HandlerLifetime.PerRequest, perRequestDescriptor.Lifetime);
            Assert.Equal(HandlerLifetime.Singleton, singletonDescriptor.Lifetime);
        }

        [SimpleExceptionFilter("filter1")]
        [SimpleCommandHandlerFilter]
        [SimpleCommandHandlerFilter]
        [SimpleCommandHandlerFilter]
        [HandlerConfiguration]
        [FakeHandlerConfiguration]
        private class SimpleCommandHandler : MessageHandler, ICommandHandler<SimpleCommand>
        {
            public void Handle(SimpleCommand command)
            {
                throw new NotImplementedException();
            }
        }

        [TransientHandler]
        private class TranscientCommandHandler : MessageHandler, ICommandHandler<SimpleCommand>
        {
            public void Handle(SimpleCommand command)
            {
                throw new NotImplementedException();
            }
        }

        [PerRequestHandler]
        private class PerRequestCommandHandler : MessageHandler, ICommandHandler<SimpleCommand>
        {
            public void Handle(SimpleCommand command)
            {
                throw new NotImplementedException();
            }
        }

        [SingletonHandler]
        private class SingletonCommandHandler : MessageHandler, ICommandHandler<SimpleCommand>
        {
            public void Handle(SimpleCommand command)
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