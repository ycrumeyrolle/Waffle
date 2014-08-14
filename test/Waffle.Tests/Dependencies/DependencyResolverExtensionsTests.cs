namespace Waffle.Tests.Dependencies
{
    using System.Linq;
    using Xunit;
    using Moq;
    using Waffle.Dependencies;
    using Waffle.Tests.Helpers;

    public class DependencyResolverExtensionsTests
    {
        readonly private Mock<IDependencyScope> resolver = new Mock<IDependencyScope>();

        [Fact]
        public void WhenGettingServiceWithoutResolverThenThrowsArgumentNullException()
        {
            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => DependencyResolverExtensions.GetService<IService>(null), "resolver");
        }

        [Fact]
        public void WhenGettingServicesWithoutResolverThenThrowsArgumentNullException()
        {
            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => DependencyResolverExtensions.GetServices<IService>(null), "resolver");
        }

        [Fact]
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
            Assert.NotNull(result);
            Assert.Same(service.Object, result);
            this.resolver.Verify(r => r.GetService(typeof(IService)), Times.Once());
        }

        [Fact]
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
            Assert.NotNull(result);
            var resultArray = result.ToArray();
            Assert.Equal(2, resultArray.Length);
            Assert.Same(service1.Object, resultArray[0]);
            Assert.Same(service2.Object, resultArray[1]);
            this.resolver.Verify(r => r.GetServices(typeof(IService)), Times.Once());
        }

        public interface IService
        {
            void Useless();
        }
    }
}