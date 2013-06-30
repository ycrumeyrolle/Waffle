namespace Waffle.Unity.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Waffle;
    using Microsoft.Practices.ObjectBuilder2;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Waffle.Tests.Helpers;

    [TestClass]
    public sealed class DependencyScopeFixture
    {
        private readonly Mock<IUnityContainer> container = new Mock<IUnityContainer>(MockBehavior.Strict);

        [TestMethod]
        public void Ctor_NullConfiguration_ThrowsArgumentNullException()
        {
            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => new FakeDependencyScope(null), "container");
        }

        [TestMethod]
        public void Ctor_ReturnsInstance()
        {
            // Arrange & act
            var scope = new FakeDependencyScope(this.container.Object);

            // Assert
            Assert.IsNotNull(scope);
        }

        [TestMethod]
        public void GetService_RegisteredService_ReturnsService()
        {
            // Arrange 
            this.container
                .Setup(c => c.Registrations)
                .Returns(new[] { CreateContainerRegistration() });
            this.container
                .Setup(c => c.Resolve(It.IsAny<Type>(), It.IsAny<string>()))
                .Returns(new object());
            DependencyScope scope = this.CreateDependencyScope();

            // Act 
            object service = scope.GetService(typeof(object));

            // Assert
            Assert.IsNotNull(service);
        }

        private static ContainerRegistration CreateContainerRegistration()
        {
            return (ContainerRegistration)Activator.CreateInstance(typeof(ContainerRegistration), BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { typeof(object), null, new PolicyList() }, null);
        }

        [TestMethod]
        public void GetService_NotRegisteredService_ReturnsNull()
        {
            // Arrange 
            this.container
                .Setup(c => c.Registrations)
                .Returns(Enumerable.Empty<ContainerRegistration>());
            DependencyScope scope = this.CreateDependencyScope();

            // Act 
            object service = scope.GetService(typeof(object));

            // Assert
            Assert.IsNull(service);
        }

        [TestMethod]
        public void GetServices_RegisteredService_ReturnsServices()
        {
            // Arrange 
            this.container
                .Setup(c => c.Registrations)
                .Returns(new[] { CreateContainerRegistration() });
            this.container
                .Setup(c => c.ResolveAll(It.IsAny<Type>()))
                .Returns(new[] { new object() });
            DependencyScope scope = this.CreateDependencyScope();

            // Act 
            IEnumerable<object> services = scope.GetServices(typeof(object));

            // Assert
            Assert.IsNotNull(services);
            Assert.AreEqual(1, services.Count());
        }

        [TestMethod]
        public void GetServices_NotRegisteredService_ReturnsEmptyArray()
        {
            // Arrange 
            this.container
                .Setup(c => c.Registrations)
                .Returns(Enumerable.Empty<ContainerRegistration>());
            DependencyScope scope = this.CreateDependencyScope();

            // Act 
            IEnumerable<object> services = scope.GetServices(typeof(object));

            // Assert
            Assert.IsNotNull(services);
            Assert.AreEqual(0, services.Count());
        }

        private DependencyScope CreateDependencyScope()
        {
            return new FakeDependencyScope(this.container.Object);
        }

        private class FakeDependencyScope : DependencyScope
        {
            public FakeDependencyScope(IUnityContainer container)
                : base(container)
            {
            }
        }
    }
}