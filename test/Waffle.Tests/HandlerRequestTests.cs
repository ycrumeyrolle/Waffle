namespace Waffle.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Moq;
    using Waffle;
    using Waffle.Commands;
    using Waffle.Dependencies;
    using Waffle.Tests.Helpers;
    using Xunit;
    
    public sealed class HandlerRequestTests : IDisposable
    {
        private readonly ICollection<IDisposable> disposableResources = new Collection<IDisposable>();

        private readonly ProcessorConfiguration defaultConfig = new ProcessorConfiguration();

        public HandlerRequestTests()
        {
            this.disposableResources.Add(this.defaultConfig);
        }

        [Fact]
        public void WhenCreatingInstanceWithNullParametersThenThrowsArgumentNullException()
        {
            // Arrange
            Mock<ICommand> command = new Mock<ICommand>();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new CommandHandlerRequest(null, command.Object), "configuration");
            ExceptionAssert.ThrowsArgumentNull(() => new CommandHandlerRequest(this.defaultConfig, null), "command");
            ExceptionAssert.ThrowsArgumentNull(() => new CommandHandlerRequest(null, null), "configuration");
        }

        [Fact]
        public void WhenCreatingInstanceThenPropertiesAreDefined()
        {
            // Arrange
            Mock<ICommand> command = new Mock<ICommand>();

            // Act
            CommandHandlerRequest request = new CommandHandlerRequest(this.defaultConfig, command.Object);

            // Assert
            Assert.Null(request.ParentRequest);
            Assert.Same(this.defaultConfig, request.Configuration);
            Assert.Same(command.Object, request.Command);
            Assert.Equal(command.Object.GetType(), request.MessageType);
            Assert.NotEqual(Guid.Empty, request.Id);
        }

        [Fact]
        public void WhenGettingDependencyScopeThenDelegatesToDependencyResolver()
        {
            // Arrange
            Mock<ICommand> command = new Mock<ICommand>();
            HandlerRequest request = new CommandHandlerRequest(this.defaultConfig, command.Object);
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
            Assert.NotNull(scope1);
            Assert.Same(scope1, scope2);
            resolver.Verify(r => r.BeginScope(), Times.Once());
        }

        [Fact]
        public void WhenGettingDependencyScopeFromDeepestRequestThenDelegatesToDeepestDependencyResolver()
        {
            // Arrange
            Mock<ICommand> command = new Mock<ICommand>();
            HandlerRequest request = new CommandHandlerRequest(this.defaultConfig, command.Object);
            HandlerRequest innerRequest = new CommandHandlerRequest(this.defaultConfig, command.Object, request);

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
            Assert.NotNull(scope1);
            Assert.NotNull(scope2);
            Assert.NotNull(scope3);
            Assert.NotNull(scope4);
            Assert.Same(scope1, scope2);
            Assert.Same(scope1, scope3);
            Assert.NotSame(scope1, scope4);
            resolver.Verify(r => r.BeginScope(), Times.Exactly(2));
        }

        [Fact]
        public void WhenDisposingThenScopeIsDisposed()
        {
            // Arrange
            Mock<ICommand> command = new Mock<ICommand>();
            HandlerRequest request = new CommandHandlerRequest(this.defaultConfig, command.Object);
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

        [Fact]
        public void WhenDisposingScopeFromDifferentsDeepsThenScopesAreDisposed()
        {
            // Arrange
            Mock<ICommand> command = new Mock<ICommand>();
            HandlerRequest request = new CommandHandlerRequest(this.defaultConfig, command.Object);
            HandlerRequest innerRequest = new CommandHandlerRequest(this.defaultConfig, command.Object, request);

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
            Assert.Equal(0, disposedCount);
            innerRequest.Dispose();
            Assert.Equal(0, disposedCount);
            request.Dispose();
            Assert.Equal(1, disposedCount);
        }

        [Fact]
        public void WhenDisposingScopeFromDeepestRequestThenScopesAreDisposed()
        {
            // Arrange
            Mock<ICommand> command = new Mock<ICommand>();
            HandlerRequest request = new CommandHandlerRequest(this.defaultConfig, command.Object);
            HandlerRequest innerRequest = new CommandHandlerRequest(this.defaultConfig, command.Object, request);

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
            Assert.Equal(0, disposedCount);
            innerRequest.Dispose();
            Assert.Equal(1, disposedCount);
            request.Dispose();
            Assert.Equal(2, disposedCount);
        }

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