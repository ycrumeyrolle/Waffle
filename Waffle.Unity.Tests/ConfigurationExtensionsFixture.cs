namespace Waffle.Unity.Tests
{
    using System;
    using Waffle;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Waffle.Tests.Helpers;

    [TestClass]
    public sealed class ConfigurationExtensionsFixture : IDisposable
    {
        private readonly ProcessorConfiguration configuration = new ProcessorConfiguration();

        public ConfigurationExtensionsFixture()
        {
            this.configuration = new ProcessorConfiguration();
        }

        [TestMethod]
        public void WhenRegisteringNullConfigurationThenThrowsArgumentNullException()
        {
            // Act & assert
            ExceptionAssert.ThrowsArgumentNull(() => ConfigurationExtensions.RegisterContainer(null, null), "configuration");
        }

        [TestMethod]
        public void WhenRegisteringContainerThenReturnsResolver()
        {
            // Arrange
            Mock<IUnityContainer> container = new Mock<IUnityContainer>();

            // Act
            var resolver = this.configuration.RegisterContainer(container.Object);

            // Assert
            Assert.IsNotNull(resolver);
        }

        [TestCleanup]
        public void Dispose()
        {
            this.configuration.Dispose();
        }
    }
}