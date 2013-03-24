namespace CommandProcessing.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using CommandProcessing;
    using CommandProcessing.Dependencies;
    using CommandProcessing.Filters;
    using CommandProcessing.Tests.Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class HandlerRequestFixture : IDisposable
    {
        private readonly ICollection<IDisposable> disposableResources = new Collection<IDisposable>();

        private readonly ProcessorConfiguration defaultConfig = new ProcessorConfiguration();

        public HandlerRequestFixture()
        {
            this.disposableResources.Add(this.defaultConfig);
        }

        [TestMethod]
        public void WhenCreatingInstanceWithNullParametersThenThrowsArgumentNullException()
        {
            // Arrange
            Mock<ICommand> command = new Mock<ICommand>();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new HandlerRequest(null, command.Object), "configuration");
            ExceptionAssert.ThrowsArgumentNull(() => new HandlerRequest(this.defaultConfig, null), "command");
            ExceptionAssert.ThrowsArgumentNull(() => new HandlerRequest(null, null), "configuration");
        }

        [TestMethod]
        public void WhenCreatingInstanceThenPropertiesAreDefined()
        {
            // Arrange
            Mock<ICommand> command = new Mock<ICommand>();

            // Act
            HandlerRequest request = new HandlerRequest(this.defaultConfig, command.Object);

            // Assert
            Assert.IsNull(request.ParentRequest);
            Assert.AreSame(this.defaultConfig, request.Configuration);
            Assert.AreSame(command.Object, request.Command);
            Assert.AreEqual(command.Object.GetType(), request.CommandType);
            Assert.AreNotEqual(Guid.Empty, request.Id);
        }

        [TestMethod]
        public void WhenGettingDependencyScopeThenDelegatesToDependencyResolver()
        {
            // Arrange
            Mock<ICommand> command = new Mock<ICommand>();
            HandlerRequest request = new HandlerRequest(this.defaultConfig, command.Object);
            Mock<IDependencyScope> scope = new Mock<IDependencyScope>(MockBehavior.Strict);
            scope.Setup(s => s.Dispose());

            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>(MockBehavior.Strict);
            resolver
                .Setup(r => r.BeginScope())
                .Returns(scope.Object);
            resolver
                .Setup(r => r.Dispose());
            this.defaultConfig.DependencyResolver = resolver.Object;

            // Act
            var scope1 = request.GetDependencyScope();
            var scope2 = request.GetDependencyScope();

            // Assert
            Assert.IsNotNull(scope1);
            Assert.AreSame(scope1, scope2);
            resolver.Verify(r => r.BeginScope(), Times.Once());
        }

        [TestMethod]
        public void WhenDisposingThenScopeIsDisposed()
        {
            // Arrange
            Mock<ICommand> command = new Mock<ICommand>();
            HandlerRequest request = new HandlerRequest(this.defaultConfig, command.Object);
            Mock<IDependencyScope> scope = new Mock<IDependencyScope>(MockBehavior.Strict);
            scope.Setup(s => s.Dispose());

            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>(MockBehavior.Strict);
            resolver
                .Setup(r => r.BeginScope())
                .Returns(scope.Object);
            resolver
                .Setup(r => r.Dispose());
            this.defaultConfig.DependencyResolver = resolver.Object;
            request.GetDependencyScope();

            // Act
            request.Dispose();

            // Assert
            scope.Verify(s => s.Dispose(), Times.Once());
        }

        [TestCleanup]
        public void Dispose()
        {
            foreach (IDisposable disposable in this.disposableResources)
            {
                disposable.Dispose();
            }
        }
    }
}