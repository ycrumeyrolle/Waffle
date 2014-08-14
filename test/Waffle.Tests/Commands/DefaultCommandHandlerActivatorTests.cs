namespace Waffle.Tests.Commands
{
    using System;
    using Xunit;
    using Moq;
    using Waffle;
    using Waffle.Commands;
    using Waffle.Dependencies;
    using Waffle.Tests.Helpers;
    using Waffle.Filters;
    
    public sealed class DefaultCommandHandlerActivatorTests : IDisposable
    {
        private readonly ProcessorConfiguration config;

        private readonly Mock<ICommand> command;

        private readonly Mock<IDependencyResolver> dependencyResolver;

        public DefaultCommandHandlerActivatorTests()
        {
            this.config = new ProcessorConfiguration();

            this.command = new Mock<ICommand>();

            this.dependencyResolver = new Mock<IDependencyResolver>(MockBehavior.Loose);
            this.dependencyResolver.Setup(r => r.BeginScope()).Returns(this.dependencyResolver.Object);
            this.config.DependencyResolver = this.dependencyResolver.Object;
        }

        [Fact]
        public void WhenCreatingHandlerWithoutParametersThenThrowsArgumentNullException()
        {
            // Assign
            ICommandHandlerActivator activator = new DefaultCommandHandlerActivator();
            CommandHandlerRequest request = new CommandHandlerRequest(this.config, this.command.Object);
            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));

            // Act
            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => activator.Create(null, null), "request");
            ExceptionAssert.ThrowsArgumentNull(() => activator.Create(null, descriptor), "request");
            ExceptionAssert.ThrowsArgumentNull(() => activator.Create(request, null), "descriptor");
        }

        [Fact]
        public void WhenCreatingHandlerFromDependencyResolverThenReturnsHandler()
        {
            // Assign
            ICommandHandlerActivator activator = new DefaultCommandHandlerActivator();
            CommandHandlerRequest request = new CommandHandlerRequest(this.config, this.command.Object);
            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            this.dependencyResolver.Setup(resolver => resolver.GetService(typeof(SimpleCommandHandler))).Returns(new SimpleCommandHandler());

            // Act
            ICommandHandler commandHandler = activator.Create(request, descriptor);

            // Assert
            Assert.NotNull(commandHandler);
            Assert.IsType(typeof(SimpleCommandHandler), commandHandler);
            Assert.Equal(0, descriptor.Properties.Count);
            this.dependencyResolver.Verify(resolver => resolver.GetService(typeof(SimpleCommandHandler)), Times.Once());
        }

        [Fact]
        public void WhenCreatingHandlerFromActivatorThenReturnsHandler()
        {
            // Assign
            ICommandHandlerActivator activator = new DefaultCommandHandlerActivator();
            CommandHandlerRequest request = new CommandHandlerRequest(this.config, this.command.Object);
            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            this.dependencyResolver.Setup(resolver => resolver.GetService(typeof(SimpleCommandHandler))).Returns(null);

            // Act
            ICommandHandler commandHandler = activator.Create(request, descriptor);

            // Assert
            Assert.NotNull(commandHandler);
            Assert.IsType(typeof(SimpleCommandHandler), commandHandler);
            Assert.Equal(0, descriptor.Properties.Count);
            this.dependencyResolver.Verify(resolver => resolver.GetService(typeof(SimpleCommandHandler)), Times.Once());
        }

        [Fact]
        public void WhenCreatingHandlerFromSameTypeTwiceThenFastCacheIsUsed()
        {
            // Assign
            ICommandHandlerActivator activator = new DefaultCommandHandlerActivator();
            CommandHandlerRequest request = new CommandHandlerRequest(this.config, this.command.Object);
            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            this.dependencyResolver.Setup(resolver => resolver.GetService(typeof(SimpleCommandHandler))).Returns(null);

            // Act
            activator.Create(request, descriptor);
            ICommandHandler commandHandler = activator.Create(request, descriptor);

            // Assert
            Assert.NotNull(commandHandler);
            Assert.IsType(typeof(SimpleCommandHandler), commandHandler);
            Assert.Equal(0, descriptor.Properties.Count);
            this.dependencyResolver.Verify(resolver => resolver.GetService(typeof(SimpleCommandHandler)), Times.Once());
        }

        [Fact]
        public void WhenCreatingHandlerFromTwoDescriptorsThenFastCacheIsNotUsed()
        {
            // Assign
            ICommandHandlerActivator activator = new DefaultCommandHandlerActivator();
            CommandHandlerRequest request1 = new CommandHandlerRequest(this.config, this.command.Object);
            CommandHandlerRequest request2 = new CommandHandlerRequest(this.config, this.command.Object);
            CommandHandlerDescriptor descriptor1 = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            CommandHandlerDescriptor descriptor2 = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            this.dependencyResolver.Setup(resolver => resolver.GetService(typeof(SimpleCommandHandler))).Returns(null);

            // Act
            activator.Create(request1, descriptor1);
            ICommandHandler commandHandler = activator.Create(request2, descriptor2);

            // Assert
            Assert.NotNull(commandHandler);
            Assert.IsType(typeof(SimpleCommandHandler), commandHandler);
            Assert.Equal(1, descriptor2.Properties.Count);
            this.dependencyResolver.Verify(resolver => resolver.GetService(typeof(SimpleCommandHandler)), Times.Exactly(2));
        }

        [Fact]
        public void WhenCreatingHandlerFromTwoDescriptorsThenGetActivatorFromProperties()
        {
            // Assign
            ICommandHandlerActivator activator = new DefaultCommandHandlerActivator();
            CommandHandlerRequest request1 = new CommandHandlerRequest(this.config, this.command.Object);
            CommandHandlerRequest request2 = new CommandHandlerRequest(this.config, this.command.Object);
            CommandHandlerDescriptor descriptor1 = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            CommandHandlerDescriptor descriptor2 = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            this.dependencyResolver.Setup(resolver => resolver.GetService(typeof(SimpleCommandHandler))).Returns(null);

            // Act
            activator.Create(request1, descriptor1);
            activator.Create(request2, descriptor2);
            ICommandHandler commandHandler = activator.Create(request2, descriptor2);

            // Assert
            Assert.NotNull(commandHandler);
            Assert.IsType(typeof(SimpleCommandHandler), commandHandler);
            Assert.Equal(1, descriptor2.Properties.Count);
            this.dependencyResolver.Verify(resolver => resolver.GetService(typeof(SimpleCommandHandler)), Times.Exactly(2));
        }

        [Fact]
        public void WhenCreatingHandlerFromTwoDescriptorsAndDependencyResolverThenGetActivatorDepencyResolver()
        {
            // Assign
            ICommandHandlerActivator activator = new DefaultCommandHandlerActivator();
            CommandHandlerRequest request = new CommandHandlerRequest(this.config, this.command.Object);
            CommandHandlerDescriptor descriptor1 = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            CommandHandlerDescriptor descriptor2 = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            int[] i = { 0 };
            this.dependencyResolver
                .When(() => i[0] < 1)
                .Setup(resolver => resolver.GetService(typeof(SimpleCommandHandler)))
                .Returns(null)
                .Callback(() => i[0]++);
            this.dependencyResolver
                .When(() => i[0] >= 1)
                .Setup(resolver => resolver.GetService(typeof(SimpleCommandHandler)))
                .Returns(new SimpleCommandHandler());

            // Act
            activator.Create(request, descriptor1);
            ICommandHandler commandHandler = activator.Create(request, descriptor2);

            // Assert
            Assert.NotNull(commandHandler);
            Assert.IsType(typeof(SimpleCommandHandler), commandHandler);
            Assert.Equal(0, descriptor2.Properties.Count);
            this.dependencyResolver.Verify(resolver => resolver.GetService(typeof(SimpleCommandHandler)), Times.Exactly(2));
        }

        [Fact]
        public void WhenCreatingHandlerThrowExceptionThenRethowsInvalidOperationException()
        {
            // Arrange
            ICommandHandlerActivator activator = new DefaultCommandHandlerActivator();
            CommandHandlerRequest request = new CommandHandlerRequest(this.config, this.command.Object);
            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));

            this.dependencyResolver
                .Setup(resolver => resolver.GetService(typeof(SimpleCommandHandler)))
                .Throws<CommandHandlerNotFoundException>();
            bool exceptionRaised = false;

            // Act
            try
            {
                activator.Create(request, descriptor);
            }
            catch (InvalidOperationException)
            {
                exceptionRaised = true;
            }

            // Assert
            Assert.True(exceptionRaised);
        }


        [Fact]
        public void CreateSingletonHandler_InstancianteOnce()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            config.DefaultHandlerLifetime = HandlerLifetime.Singleton;
            ICommandHandlerActivator activator = new DefaultCommandHandlerActivator();
            CommandHandlerRequest request1 = new CommandHandlerRequest(config, this.command.Object);
            CommandHandlerRequest request2 = new CommandHandlerRequest(config, this.command.Object);
            CommandHandlerDescriptor descriptor1 = new CommandHandlerDescriptor(config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            CommandHandlerDescriptor descriptor2 = new CommandHandlerDescriptor(config, typeof(SimpleCommand), typeof(SimpleCommandHandler));

            // Act
            var handler1 = activator.Create(request1, descriptor1);
            var handler2 = activator.Create(request1, descriptor1);
            var handler3 = activator.Create(request2, descriptor1);
            var handler4 = activator.Create(request1, descriptor2);

            // Assert
            Assert.NotNull(handler1);
            Assert.NotNull(handler2);
            Assert.NotNull(handler3);
            Assert.NotNull(handler4);
            Assert.Same(handler1, handler2);
            Assert.Same(handler1, handler3);
            Assert.Same(handler1, handler4);
        }

        [Fact]
        public void CreatePerRequestHandler_InstancianteOncePerRequest()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            config.DefaultHandlerLifetime = HandlerLifetime.PerRequest;
            ICommandHandlerActivator activator = new DefaultCommandHandlerActivator();
            CommandHandlerRequest request1 = new CommandHandlerRequest(config, this.command.Object);
            CommandHandlerRequest request2 = new CommandHandlerRequest(config, this.command.Object);
            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            CommandHandlerDescriptor descriptor2 = new CommandHandlerDescriptor(config, typeof(SimpleCommand), typeof(SimpleCommandHandler));

            // Act
            var handler1 = activator.Create(request1, descriptor);
            var handler2 = activator.Create(request1, descriptor);
            var handler3 = activator.Create(request2, descriptor);
            var handler4 = activator.Create(request1, descriptor2);

            // Assert
            Assert.NotNull(handler1);
            Assert.NotNull(handler2);
            Assert.NotNull(handler3);
            Assert.NotNull(handler4);
            Assert.Same(handler1, handler2);
            Assert.NotSame(handler1, handler3);
            Assert.Same(handler1, handler4);
        }

        [Fact]
        public void CreateTranscientHandler_InstancianteEachTime()
        {
            // Arrange
            var config = new ProcessorConfiguration();
            config.DefaultHandlerLifetime = HandlerLifetime.Transient;
            ICommandHandlerActivator activator = new DefaultCommandHandlerActivator();
            CommandHandlerRequest request1 = new CommandHandlerRequest(config, this.command.Object);
            CommandHandlerRequest request2 = new CommandHandlerRequest(config, this.command.Object);
            CommandHandlerDescriptor descriptor = new CommandHandlerDescriptor(config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            CommandHandlerDescriptor descriptor2 = new CommandHandlerDescriptor(config, typeof(SimpleCommand), typeof(SimpleCommandHandler));

            // Act
            var handler1 = activator.Create(request1, descriptor);
            var handler2 = activator.Create(request1, descriptor);
            var handler3 = activator.Create(request2, descriptor);
            var handler4 = activator.Create(request1, descriptor2);

            // Assert
            Assert.NotNull(handler1);
            Assert.NotNull(handler2);
            Assert.NotNull(handler3);
            Assert.NotNull(handler4);
            Assert.NotSame(handler1, handler2);
            Assert.NotSame(handler1, handler3);
            Assert.NotSame(handler1, handler4);
            Assert.NotSame(handler2, handler3);
            Assert.NotSame(handler2, handler4);
            Assert.NotSame(handler3, handler4);
        }

        public void Dispose()
        {
            this.config.Dispose();
        }
    }
}