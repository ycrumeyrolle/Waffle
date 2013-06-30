namespace Waffle.Tests.Interception
{
    using System;
    using Waffle;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Waffle.Interception;

    [TestClass]
    public sealed class DefaultInterceptionProviderFixture : IDisposable
    {
        private readonly ProcessorConfiguration configuration;

        private readonly Mock<IInterceptor> interceptor;

        public DefaultInterceptionProviderFixture()
        {
            this.configuration = new ProcessorConfiguration();
            this.interceptor = new Mock<IInterceptor>();
         
            this.interceptor.Setup(i => i.OnExecuting());
            this.interceptor.Setup(i => i.OnExecuted());
            this.interceptor.Setup(i => i.OnException(It.IsAny<Exception>()));

            this.configuration.Services.Add(typeof(IInterceptor), this.interceptor.Object);
        }

        [TestMethod]
        public void WhenCallingExecutingMethodThenCallInterceptor()
        {
            // Assign
            DefaultInterceptionProvider provider = new DefaultInterceptionProvider(this.configuration);

            // Act
            provider.OnExecuting();

            // Assert
            this.interceptor.Verify(i => i.OnExecuting(), Times.Once());
            this.interceptor.Verify(i => i.OnExecuted(), Times.Never());
            this.interceptor.Verify(i => i.OnException(It.IsAny<Exception>()), Times.Never());
        }

        [TestMethod]
        public void WhenCallingExecutedMethodThenCallInterceptor()
        {
            // Assign
            DefaultInterceptionProvider provider = new DefaultInterceptionProvider(this.configuration);

            // Act
            provider.OnExecuted();

            // Assert
            this.interceptor.Verify(i => i.OnExecuting(), Times.Never());
            this.interceptor.Verify(i => i.OnExecuted(), Times.Once());
            this.interceptor.Verify(i => i.OnException(It.IsAny<Exception>()), Times.Never());
        }

        [TestMethod]
        public void WhenCallingExceptionMethodThenCallInterceptor()
        {
            // Assign
            DefaultInterceptionProvider provider = new DefaultInterceptionProvider(this.configuration);

            // Act
            provider.OnException(new Exception());

            // Assert
            this.interceptor.Verify(i => i.OnExecuting(), Times.Never());
            this.interceptor.Verify(i => i.OnExecuted(), Times.Never());
            this.interceptor.Verify(i => i.OnException(It.IsAny<Exception>()), Times.Once());
        }

        [TestCleanup]
        public void Dispose()
        {
            this.configuration.Dispose();
        }
    }
}