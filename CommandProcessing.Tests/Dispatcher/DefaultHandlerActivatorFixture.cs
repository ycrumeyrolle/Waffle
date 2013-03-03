namespace CommandProcessing.Tests.Dispatcher
{
    using System;

    using CommandProcessing;
    using CommandProcessing.Dependencies;
    using CommandProcessing.Dispatcher;
    using CommandProcessing.Filters;
    using CommandProcessing.Tests.Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class DefaultHandlerActivatorFixture : IDisposable
    {
        private readonly ProcessorConfiguration config;

        private readonly Mock<ICommand> command;

        private readonly Mock<IDependencyResolver> dependencyResolver;

        public DefaultHandlerActivatorFixture()
        {
            this.config = new ProcessorConfiguration();

            this.command = new Mock<ICommand>();

            this.dependencyResolver = new Mock<IDependencyResolver>(MockBehavior.Loose);
            this.dependencyResolver.Setup(r => r.BeginScope()).Returns(this.dependencyResolver.Object);

            // this.dependencyResolver
            // .Setup(resolver => resolver.GetService(It.IsAny<Type>()))
            // .Returns(null);
            this.config.DependencyResolver = this.dependencyResolver.Object;
        }

        [TestMethod]
        public void WhenCreatingHandlerWithoutParametersThenThrowsArgumentNullException()
        {
            // Assign
            IHandlerActivator activator = new DefaultHandlerActivator();
            HandlerRequest request = new HandlerRequest(this.config, this.command.Object);
            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleHandler));
            bool exceptionRaised = false;

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
            IHandlerActivator activator = new DefaultHandlerActivator();
            HandlerRequest request = new HandlerRequest(this.config, this.command.Object);
            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleHandler));
            this.dependencyResolver.Setup(resolver => resolver.GetService(typeof(SimpleHandler))).Returns(new SimpleHandler());

            // Act
            ICommandHandler handler = activator.Create(request, descriptor);

            // Assert
            Assert.IsNotNull(handler);
            Assert.IsInstanceOfType(handler, typeof(SimpleHandler));
            this.dependencyResolver.Verify(resolver => resolver.GetService(typeof(SimpleHandler)), Times.Once());
        }

        [TestMethod]
        public void WhenCreatingHandlerFromDependencyResolverTwiceThenFastCacheIsUsed()
        {
            // Assign
            IHandlerActivator activator = new DefaultHandlerActivator();
            HandlerRequest request = new HandlerRequest(this.config, this.command.Object);
            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleHandler));
            this.dependencyResolver.Setup(resolver => resolver.GetService(typeof(SimpleHandler))).Returns(null);

            // Act
            activator.Create(request, descriptor);
            ICommandHandler handler = activator.Create(request, descriptor);

            // Assert
            Assert.IsNotNull(handler);
            Assert.IsInstanceOfType(handler, typeof(SimpleHandler));
            this.dependencyResolver.Verify(resolver => resolver.GetService(typeof(SimpleHandler)), Times.Once());
        }

        [TestMethod]
        public void WhenCreatingHandlerFromActivatorThenReturnsHandler()
        {
            // Assign
            IHandlerActivator activator = new DefaultHandlerActivator();
            HandlerRequest request = new HandlerRequest(this.config, this.command.Object);
            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleHandler));
            this.dependencyResolver.Setup(resolver => resolver.GetService(typeof(SimpleHandler))).Returns(null);

            // Act
            ICommandHandler handler = activator.Create(request, descriptor);

            // Assert
            Assert.IsNotNull(handler);
            Assert.IsInstanceOfType(handler, typeof(SimpleHandler));
            this.dependencyResolver.Verify(resolver => resolver.GetService(typeof(SimpleHandler)), Times.Once());
        }

        [TestMethod]
        public void WhenCreatingHandlerFromSameTypeTwiceThenFastCacheIsUsed()
        {
            // Assign
            IHandlerActivator activator = new DefaultHandlerActivator();
            HandlerRequest request = new HandlerRequest(this.config, this.command.Object);
            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleHandler));
            this.dependencyResolver.Setup(resolver => resolver.GetService(typeof(SimpleHandler))).Returns(null);

            // Act
            ICommandHandler handler = activator.Create(request, descriptor);

            // Assert
            Assert.IsNotNull(handler);
            Assert.IsInstanceOfType(handler, typeof(SimpleHandler));
            this.dependencyResolver.Verify(resolver => resolver.GetService(typeof(SimpleHandler)), Times.Once());
        }

        [TestMethod]
        public void WhenCreatingHandlerFromSameTypeTwiceThenFastCacheIsUsed2()
        {
            // Assign
            IHandlerActivator activator = new DefaultHandlerActivator();
            HandlerRequest request = new HandlerRequest(this.config, this.command.Object);
            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleHandler));
            this.dependencyResolver.Setup(resolver => resolver.GetService(typeof(SimpleHandler))).Returns(null);

            // Act
            ICommandHandler handler = activator.Create(request, descriptor);

            // Assert
            Assert.IsNotNull(handler);
            Assert.IsInstanceOfType(handler, typeof(SimpleHandler));
            this.dependencyResolver.Verify(resolver => resolver.GetService(typeof(SimpleHandler)), Times.Once());
        }
        
        [TestCleanup]
        public void Dispose()
        {
            this.config.Dispose();
        }
    }
}