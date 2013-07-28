namespace Waffle.Unity.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
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

            IBuilderContext builderContext = new FakeBuildContext();
    
            this.container
                .Setup(c => c.Resolve(It.IsAny<Type>(), It.IsAny<string>()))
                .Throws(new ResolutionFailedException(typeof(object), null, null, builderContext));

            DependencyScope scope = this.CreateDependencyScope();

            // Act 
            object service1 = scope.GetService(typeof(IAssembliesResolver));
            object service2 = scope.GetService(typeof(IAssembliesResolver));

            // Assert
            Assert.IsNull(service1);
            Assert.IsNull(service2);

            // Assert the resolution is done only once when it throw a ResolutionFailedException
            this.container.Verify(c => c.Resolve(It.IsAny<Type>(), It.IsAny<string>()), Times.Once());
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
            IBuilderContext builderContext = new FakeBuildContext();

            this.container
                .Setup(c => c.ResolveAll(It.IsAny<Type>()))
                .Throws(new ResolutionFailedException(typeof(object), null, null, builderContext));
            DependencyScope scope = this.CreateDependencyScope();

            // Act 
            IEnumerable<object> services1 = scope.GetServices(typeof(IAssembliesResolver));
            IEnumerable<object> services2 = scope.GetServices(typeof(IAssembliesResolver));

            // Assert
            Assert.IsNotNull(services1);
            Assert.AreEqual(0, services1.Count());
            Assert.IsNotNull(services2);
            Assert.AreEqual(0, services2.Count());

            // Assert the resolution is done only once when it throw a ResolutionFailedException
            this.container.Verify(c => c.ResolveAll(It.IsAny<Type>()), Times.Once());
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

        private class FakeBuildContext : IBuilderContext
        {
            public void AddResolverOverrides(IEnumerable<ResolverOverride> newOverrides)
            {
            }

            public IDependencyResolverPolicy GetOverriddenResolver(Type dependencyType)
            {
                return null;
            }

            public object NewBuildUp(NamedTypeBuildKey newBuildKey)
            {
                return null;
            }

            public object NewBuildUp(NamedTypeBuildKey newBuildKey, Action<IBuilderContext> childCustomizationBlock)
            {
                return null;
            }

            public IStrategyChain Strategies { get; private set; }

            public ILifetimeContainer Lifetime { get; private set; }

            public NamedTypeBuildKey OriginalBuildKey {
                get
                {
                    return new NamedTypeBuildKey(typeof(object));
                }
            }

            public NamedTypeBuildKey BuildKey
            {
                get
                {
                    return new NamedTypeBuildKey(typeof(object));
                }

                set
                {
                }
            }

            public IPolicyList PersistentPolicies { get; private set; }

            public IPolicyList Policies { get; private set; }

            public IRecoveryStack RecoveryStack { get; private set; }

            public object Existing { get; set; }

            public bool BuildComplete { get; set; }

            public object CurrentOperation { get; set; }

            public IBuilderContext ChildContext { get; private set; }
        }
    }
}