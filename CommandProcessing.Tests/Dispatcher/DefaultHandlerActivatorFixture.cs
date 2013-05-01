namespace CommandProcessing.Tests.Dispatcher
{
    using System;
    using System.Linq;

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
            this.config.DependencyResolver = this.dependencyResolver.Object;
        }

        [TestMethod]
        public void WhenCreatingHandlerWithoutParametersThenThrowsArgumentNullException()
        {
            // Assign
            IHandlerActivator activator = new DefaultHandlerActivator();
            HandlerRequest request = new HandlerRequest(this.config, this.command.Object);
            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleHandler));

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
            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleHandler));
            this.dependencyResolver.Setup(resolver => resolver.GetService(typeof(SimpleHandler))).Returns(new SimpleHandler());

            // Act
            IHandler handler = activator.Create(request, descriptor);

            // Assert
            Assert.IsNotNull(handler);
            Assert.IsInstanceOfType(handler, typeof(SimpleHandler));
            Assert.AreEqual(0, descriptor.Properties.Count);
            this.dependencyResolver.Verify(resolver => resolver.GetService(typeof(SimpleHandler)), Times.Once());
        }

        [TestMethod]
        public void WhenCreatingHandlerFromDependencyResolverTwiceThenFastCacheIsUsed()
        {
            // Assign
            IHandlerActivator activator = new DefaultHandlerActivator();
            HandlerRequest request = new HandlerRequest(this.config, this.command.Object);
            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleHandler));
            this.dependencyResolver.Setup(resolver => resolver.GetService(typeof(SimpleHandler))).Returns(null);

            // Act
            activator.Create(request, descriptor);
            IHandler handler = activator.Create(request, descriptor);

            // Assert
            Assert.IsNotNull(handler);
            Assert.IsInstanceOfType(handler, typeof(SimpleHandler));
            Assert.AreEqual(0, descriptor.Properties.Count);
            this.dependencyResolver.Verify(resolver => resolver.GetService(typeof(SimpleHandler)), Times.Once());
        }

        [TestMethod]
        public void WhenCreatingHandlerFromActivatorThenReturnsHandler()
        {
            // Assign
            IHandlerActivator activator = new DefaultHandlerActivator();
            HandlerRequest request = new HandlerRequest(this.config, this.command.Object);
            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleHandler));
            this.dependencyResolver.Setup(resolver => resolver.GetService(typeof(SimpleHandler))).Returns(null);

            // Act
            IHandler handler = activator.Create(request, descriptor);

            // Assert
            Assert.IsNotNull(handler);
            Assert.IsInstanceOfType(handler, typeof(SimpleHandler));
            Assert.AreEqual(0, descriptor.Properties.Count);
            this.dependencyResolver.Verify(resolver => resolver.GetService(typeof(SimpleHandler)), Times.Once());
        }

        [TestMethod]
        public void WhenCreatingHandlerFromSameTypeTwiceThenFastCacheIsUsed()
        {
            // Assign
            IHandlerActivator activator = new DefaultHandlerActivator();
            HandlerRequest request = new HandlerRequest(this.config, this.command.Object);
            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleHandler));
            this.dependencyResolver.Setup(resolver => resolver.GetService(typeof(SimpleHandler))).Returns(null);

            // Act
            activator.Create(request, descriptor);
            IHandler handler = activator.Create(request, descriptor);

            // Assert
            Assert.IsNotNull(handler);
            Assert.IsInstanceOfType(handler, typeof(SimpleHandler));
            Assert.AreEqual(0, descriptor.Properties.Count);
            this.dependencyResolver.Verify(resolver => resolver.GetService(typeof(SimpleHandler)), Times.Once());
        }

        [TestMethod]
        public void WhenCreatingHandlerFromTwoDescriptorsThenFastCacheIsNotUsed()
        {
            // Assign
            IHandlerActivator activator = new DefaultHandlerActivator();
            HandlerRequest request = new HandlerRequest(this.config, this.command.Object);
            HandlerDescriptor descriptor1 = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleHandler));
            HandlerDescriptor descriptor2 = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleHandler));
            this.dependencyResolver.Setup(resolver => resolver.GetService(typeof(SimpleHandler))).Returns(null);

            // Act
            activator.Create(request, descriptor1);
            IHandler handler = activator.Create(request, descriptor2);

            // Assert
            Assert.IsNotNull(handler);
            Assert.IsInstanceOfType(handler, typeof(SimpleHandler));
            Assert.AreEqual(1, descriptor2.Properties.Count);
            this.dependencyResolver.Verify(resolver => resolver.GetService(typeof(SimpleHandler)), Times.Exactly(2));
        }

        [TestMethod]
        public void WhenCreatingHandlerFromTwoDescriptorsThenGetActivatorFromProperties()
        {
            // Assign
            IHandlerActivator activator = new DefaultHandlerActivator();
            HandlerRequest request = new HandlerRequest(this.config, this.command.Object);
            HandlerDescriptor descriptor1 = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleHandler));
            HandlerDescriptor descriptor2 = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleHandler));
            this.dependencyResolver.Setup(resolver => resolver.GetService(typeof(SimpleHandler))).Returns(null);

            // Act
            activator.Create(request, descriptor1);
            activator.Create(request, descriptor2);
            IHandler handler = activator.Create(request, descriptor2);

            // Assert
            Assert.IsNotNull(handler);
            Assert.IsInstanceOfType(handler, typeof(SimpleHandler));
            Assert.AreEqual(1, descriptor2.Properties.Count);
            this.dependencyResolver.Verify(resolver => resolver.GetService(typeof(SimpleHandler)), Times.Exactly(2));
        }

        [TestMethod]
        public void WhenCreatingHandlerFromTwoDescriptorsAndDependencyResolverThenGetActivatorDepencyResolver()
        {
            // Assign
            IHandlerActivator activator = new DefaultHandlerActivator();
            HandlerRequest request = new HandlerRequest(this.config, this.command.Object);
            HandlerDescriptor descriptor1 = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleHandler));
            HandlerDescriptor descriptor2 = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleHandler));
            int[] i = { 0 };
            this.dependencyResolver
                .When(() => i[0] < 1)
                .Setup(resolver => resolver.GetService(typeof(SimpleHandler)))
                .Returns(null)
                .Callback(() => i[0]++);
            this.dependencyResolver
                .When(() => i[0] >= 1)
                .Setup(resolver => resolver.GetService(typeof(SimpleHandler)))
                .Returns(new SimpleHandler());

            // Act
            activator.Create(request, descriptor1);
            IHandler handler = activator.Create(request, descriptor2);

            // Assert
            Assert.IsNotNull(handler);
            Assert.IsInstanceOfType(handler, typeof(SimpleHandler));
            Assert.AreEqual(0, descriptor2.Properties.Count);
            this.dependencyResolver.Verify(resolver => resolver.GetService(typeof(SimpleHandler)), Times.Exactly(2));
        }

        [TestMethod]
        public void WhenCreatingHandlerThrowExceptionThenRethowsInvalidOperationException()
        {
            // Assign
            IHandlerActivator activator = new DefaultHandlerActivator();
            HandlerRequest request = new HandlerRequest(this.config, this.command.Object);
            HandlerDescriptor descriptor = new HandlerDescriptor(this.config, typeof(SimpleCommand), typeof(SimpleHandler));

            this.dependencyResolver
                .Setup(resolver => resolver.GetService(typeof(SimpleHandler)))
                .Throws<HandlerNotFoundException>();
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