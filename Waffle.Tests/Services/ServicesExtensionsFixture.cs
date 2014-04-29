namespace Waffle.Tests.Services
{
    using System;
    using Xunit;
    using Waffle;
    using Waffle.Commands;
    using Waffle.Services;
    using Waffle.Tests.Helpers;

    
    public class ServicesExtensionsFixture
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