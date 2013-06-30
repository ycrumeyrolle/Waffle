namespace Waffle.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Waffle;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Waffle.Dependencies;
    using Waffle.Tests.Helpers;

    [TestClass]
    public sealed class HandlerRequestFixture : IDisposable
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
        public void WhenGettingDependencyScopeFromDeepestRequestThenDelegatesToDeepestDependencyResolver()
        {
            // Arrange
            Mock<ICommand> command = new Mock<ICommand>();
            HandlerRequest request = new HandlerRequest(this.defaultConfig, command.Object);
            HandlerRequest innerRequest = new HandlerRequest(this.defaultConfig, command.Object, request);

            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>(MockBehavior.Strict);
            resolver
                .Setup(r => r.BeginScope())
                .Returns(CreateScopeMock);
            resolver
                .Setup(r => r.Dispose());
            this.defaultConfig.DependencyResolver = resolver.Object;

            // Act
            var scope1 = request.GetDependencyScope();
            var scope2 = request.GetDependencyScope(false);
            var scope3 = innerRequest.GetDependencyScope();
            var scope4 = innerRequest.GetDependencyScope(false);

            // Assert
            Assert.IsNotNull(scope1);
            Assert.IsNotNull(scope2);
            Assert.IsNotNull(scope3);
            Assert.IsNotNull(scope4);
            Assert.AreSame(scope1, scope2);
            Assert.AreSame(scope1, scope3);
            Assert.AreNotSame(scope1, scope4);
            resolver.Verify(r => r.BeginScope(), Times.Exactly(2));
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

        [TestMethod]
        public void WhenDisposingScopeFromDifferentsDeepsThenScopesAreDisposed()
        {
            // Arrange
            Mock<ICommand> command = new Mock<ICommand>();
            HandlerRequest request = new HandlerRequest(this.defaultConfig, command.Object);
            HandlerRequest innerRequest = new HandlerRequest(this.defaultConfig, command.Object, request);

            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>(MockBehavior.Strict);
            int disposedCount = 0;
            resolver
                .Setup(r => r.BeginScope())
                .Returns(() => CreateVerifiableScopeMock(() => disposedCount++));
            resolver
                .Setup(r => r.Dispose());
            this.defaultConfig.DependencyResolver = resolver.Object;

            request.GetDependencyScope();
            innerRequest.GetDependencyScope();

            // Act & Assert
            Assert.AreEqual(0, disposedCount);
            innerRequest.Dispose();
            Assert.AreEqual(0, disposedCount);
            request.Dispose();
            Assert.AreEqual(1, disposedCount);
        }

        [TestMethod]
        public void WhenDisposingScopeFromDeepestRequestThenScopesAreDisposed()
        {
            // Arrange
            Mock<ICommand> command = new Mock<ICommand>();
            HandlerRequest request = new HandlerRequest(this.defaultConfig, command.Object);
            HandlerRequest innerRequest = new HandlerRequest(this.defaultConfig, command.Object, request);

            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>(MockBehavior.Strict);
            int disposedCount = 0;
            resolver
                .Setup(r => r.BeginScope())
                .Returns(() => CreateVerifiableScopeMock(() => disposedCount++));
            resolver
                .Setup(r => r.Dispose());
            this.defaultConfig.DependencyResolver = resolver.Object;

            request.GetDependencyScope();
            innerRequest.GetDependencyScope(false);

            // Act & Assert
            Assert.AreEqual(0, disposedCount);
            innerRequest.Dispose();
            Assert.AreEqual(1, disposedCount);
            request.Dispose();
            Assert.AreEqual(2, disposedCount);
        }

        [TestCleanup]
        public void Dispose()
        {
            this.defaultConfig.Dispose();
            foreach (IDisposable disposable in this.disposableResources)
            {
                disposable.Dispose();
            }
        }

        private static IDependencyScope CreateScopeMock()
        {
            Mock<IDependencyScope> scope = new Mock<IDependencyScope>(MockBehavior.Strict);
            scope.Setup(s => s.Dispose());
            return scope.Object;
        }

        private static IDependencyScope CreateVerifiableScopeMock(Action action)
        {
            Mock<IDependencyScope> scope = new Mock<IDependencyScope>(MockBehavior.Strict);
            scope.Setup(s => s.Dispose()).Callback(() => action());
            return scope.Object;
        }
    }
}