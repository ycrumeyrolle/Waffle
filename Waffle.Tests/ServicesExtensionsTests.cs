namespace Waffle.Tests
{
    using System;
    using Waffle.Commands;
    using Waffle.Services;
    using Xunit;

    public class ServicesExtensionsTests
    {
        [Fact]
        public void WhenGettingOrThrowUnknowServiceThenThrowsInvalidOperationException()
        {
            ProcessorConfiguration config = new ProcessorConfiguration();
            DefaultServices services = new DefaultServices(config);
            services.Replace(typeof(ICommandHandlerActivator), null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => services.GetServiceOrThrow<ICommandHandlerActivator>());
        }
    }
}