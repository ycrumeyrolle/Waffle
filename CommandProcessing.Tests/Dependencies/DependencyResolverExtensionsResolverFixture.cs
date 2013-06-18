namespace CommandProcessing.Tests.Dependencies
{
    using System.Linq;
    using CommandProcessing.Dependencies;
    using CommandProcessing.Tests.Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class DependencyResolverExtensionsResolverFixture
    {
        readonly private Mock<IDependencyScope> resolver = new Mock<IDependencyScope>();

        [TestMethod]
        public void WhenGettingServiceWithoutResolverThenThrowsArgumentNullException()
        {
            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => DependencyResolverExtensions.GetService<IService>(null), "resolver");
        }

        [TestMethod]
        public void WhenGettingServicesWithoutResolverThenThrowsArgumentNullException()
        {
            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => DependencyResolverExtensions.GetServices<IService>(null), "resolver");
        }

        [TestMethod]
        public void WhenGettingServiceThenDelegatesToResolver()
        {
            // Arrange
            Mock<IService> service = new Mock<IService>();
            this.resolver
                .Setup(r => r.GetService(typeof(IService)))
                .Returns(service.Object);

            // Act
            var result = DependencyResolverExtensions.GetService<IService>(this.resolver.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreSame(service.Object, result);
            this.resolver.Verify(r => r.GetService(typeof(IService)), Times.Once());
        }

        [TestMethod]
        public void WhenGettingServicesThenDelegatesToResolver()
        {
            // Arrange
            Mock<IService> service1 = new Mock<IService>();
            Mock<IService> service2 = new Mock<IService>();
            this.resolver
                .Setup(r => r.GetServices(typeof(IService)))
                .Returns(new[] { service1.Object, service2.Object });

            // Act
            var result = DependencyResolverExtensions.GetServices<IService>(this.resolver.Object);

            // Assert
            Assert.IsNotNull(result);
            var resultArray = result.ToArray();
            Assert.AreEqual(2, resultArray.Length);
            Assert.AreSame(service1.Object, resultArray[0]);
            Assert.AreSame(service2.Object, resultArray[1]);
            this.resolver.Verify(r => r.GetServices(typeof(IService)), Times.Once());
        }

        public interface IService
        {
            void Useless();
        }
    }
}