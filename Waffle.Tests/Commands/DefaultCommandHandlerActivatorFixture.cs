namespace Waffle.Tests.Commands
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Waffle;
    using Waffle.Commands;
    using Waffle.Dependencies;
    using Waffle.Tests.Helpers;

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
        public void WhenCreatingHandlerFromDependencyResolverTwiceThenFastCacheIsUsed()
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
            CommandHandlerRequest request = new CommandHandlerRequest(this.config, this.command.Object);
            CommandHandlerDescriptor descriptor1 = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            CommandHandlerDescriptor descriptor2 = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            this.dependencyResolver.Setup(resolver => resolver.GetService(typeof(SimpleCommandHandler))).Returns(null);

            // Act
            activator.Create(request, descriptor1);
            ICommandHandler commandHandler = activator.Create(request, descriptor2);

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
            CommandHandlerRequest request = new CommandHandlerRequest(this.config, this.command.Object);
            CommandHandlerDescriptor descriptor1 = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            CommandHandlerDescriptor descriptor2 = new CommandHandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleCommandHandler));
            this.dependencyResolver.Setup(resolver => resolver.GetService(typeof(SimpleCommandHandler))).Returns(null);

            // Act
            activator.Create(request, descriptor1);
            activator.Create(request, descriptor2);
            ICommandHandler commandHandler = activator.Create(request, descriptor2);

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
            // Assign
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

        [TestCleanup]
        public void Dispose()
        {
            this.config.Dispose();
        }
    }
}