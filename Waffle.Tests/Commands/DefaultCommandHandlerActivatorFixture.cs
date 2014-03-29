namespace Waffle.Tests.Commands
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Waffle;
    using Waffle.Commands;
    using Waffle.Dependencies;
    using Waffle.Tests.Helpers;
    using Waffle.Filters;

    [TestClass]
    public sealed class DefaultCommandHandlerActivatorFixture : IDisposable
    {
        private readonly ProcessorConfiguration config;

        private readonly Mock<ICommand> command;

        private readonly Mock<IDependencyResolver> dependencyResolver;

        public DefaultCommandHandlerActivatorFixture()
        {
            this.config = new ProcessorConfiguration();

            this.command = new Mock<ICommand>();

            this.dependencyResolver = new Mock<IDependencyResolver>(MockBehavior.Loose);
            this.dependencyResolver.Setup(r => r.BeginScope()).Returns(this.dependencyResolver.Object);
            this.config.DependencyResolver = this.dependencyResolver.Object;
        }

        [TestMethod]
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

        [TestMethod]
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
            Assert.IsNotNull(commandHandler);
            Assert.IsInstanceOfType(commandHandler, typeof(SimpleCommandHandler));
            Assert.AreEqual(0, descriptor.Properties.Count);
            this.dependencyResolver.Verify(resolver => resolver.GetService(typeof(SimpleCommandHandler)), Times.Once());
        }

        [TestMethod]
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
            Assert.IsNotNull(commandHandler);
            Assert.IsInstanceOfType(commandHandler, typeof(SimpleCommandHandler));
            Assert.AreEqual(0, descriptor.Properties.Count);
            this.dependencyResolver.Verify(resolver => resolver.GetService(typeof(SimpleCommandHandler)), Times.Once());
        }

        [TestMethod]
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
            Assert.IsNotNull(commandHandler);
            Assert.IsInstanceOfType(commandHandler, typeof(SimpleCommandHandler));
            Assert.AreEqual(0, descriptor.Properties.Count);
            this.dependencyResolver.Verify(resolver => resolver.GetService(typeof(SimpleCommandHandler)), Times.Once());
        }

        [TestMethod]
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
            Assert.IsNotNull(commandHandler);
            Assert.IsInstanceOfType(commandHandler, typeof(SimpleCommandHandler));
            Assert.AreEqual(1, descriptor2.Properties.Count);
            this.dependencyResolver.Verify(resolver => resolver.GetService(typeof(SimpleCommandHandler)), Times.Exactly(2));
        }

        [TestMethod]
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
            Assert.IsNotNull(commandHandler);
            Assert.IsInstanceOfType(commandHandler, typeof(SimpleCommandHandler));
            Assert.AreEqual(1, descriptor2.Properties.Count);
            this.dependencyResolver.Verify(resolver => resolver.GetService(typeof(SimpleCommandHandler)), Times.Exactly(2));
        }

        [TestMethod]
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
            Assert.IsNotNull(commandHandler);
            Assert.IsInstanceOfType(commandHandler, typeof(SimpleCommandHandler));
            Assert.AreEqual(0, descriptor2.Properties.Count);
            this.dependencyResolver.Verify(resolver => resolver.GetService(typeof(SimpleCommandHandler)), Times.Exactly(2));
        }

        [TestMethod]
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
            Assert.IsTrue(exceptionRaised);
        }


        [TestMethod]
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
            Assert.IsNotNull(handler1);
            Assert.IsNotNull(handler2);
            Assert.IsNotNull(handler3);
            Assert.IsNotNull(handler4);
            Assert.AreSame(handler1, handler2);
            Assert.AreSame(handler1, handler3);
            Assert.AreSame(handler1, handler4);
        }

        [TestMethod]
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
            Assert.IsNotNull(handler1);
            Assert.IsNotNull(handler2);
            Assert.IsNotNull(handler3);
            Assert.IsNotNull(handler4);
            Assert.AreSame(handler1, handler2);
            Assert.AreNotSame(handler1, handler3);
            Assert.AreSame(handler1, handler4);
        }

        [TestMethod]
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
            Assert.IsNotNull(handler1);
            Assert.IsNotNull(handler2);
            Assert.IsNotNull(handler3);
            Assert.IsNotNull(handler4);
            Assert.AreNotSame(handler1, handler2);
            Assert.AreNotSame(handler1, handler3);
            Assert.AreNotSame(handler1, handler4);
            Assert.AreNotSame(handler2, handler3);
            Assert.AreNotSame(handler2, handler4);
            Assert.AreNotSame(handler3, handler4);
        }

        [TestCleanup]
        public void Dispose()
        {
            this.config.Dispose();
        }
    }
}