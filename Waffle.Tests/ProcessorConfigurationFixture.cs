namespace Waffle.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Waffle;
    using Xunit;
    using Moq;
    using Waffle.Tests.Helpers;

    
    public sealed class ProcessorConfigurationFixture : IDisposable
    {
        private readonly ICollection<IDisposable> disposableResources = new Collection<IDisposable>();

        private readonly ProcessorConfiguration defaultConfig = new ProcessorConfiguration();

        public ProcessorConfigurationFixture()
        {
            this.disposableResources.Add(this.defaultConfig);
        }

        [Fact]
        public void WhenCreatingProcessorWithDefaultCtorThenPropertiesAreDefined()
        {
            // Act
            ProcessorConfiguration config = new ProcessorConfiguration();
            this.disposableResources.Add(config);

            // Assert
            Assert.True(config.AbortOnInvalidCommand);
            Assert.False(config.ServiceProxyCreationEnabled);
            Assert.NotNull(config.DependencyResolver);
            Assert.NotNull(config.Services);
            Assert.NotNull(config.Initializer);
            Assert.Equal(0, config.Filters.Count);
        }

        [Fact]
        public void WhenSettingNullValuesThenThrowsArgumentNullException()
        {
            // Arrange
            ProcessorConfiguration config = new ProcessorConfiguration();
            this.disposableResources.Add(config);
            
            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => config.Initializer = null, "value");
            ExceptionAssert.ThrowsArgumentNull(() => config.DependencyResolver = null, "value");
        }

        [Fact]
        public void WhenRegisteringResourceToDisposeThenResouseIsDisposed()
        {
            // Arrange
            Mock<IDisposable> disposable = new Mock<IDisposable>();
            ProcessorConfiguration config = new ProcessorConfiguration();
            disposable.Setup(d => d.Dispose());
            config.RegisterForDispose(disposable.Object);

            // Act
            config.Dispose();

            // Assert
            disposable.Verify(d => d.Dispose(), Times.Once());
        }
        
        public void Dispose()
        {
            this.defaultConfig.Dispose();
            foreach (IDisposable disposable in this.disposableResources)
            {
                disposable.Dispose();
            }
        }
    }
}