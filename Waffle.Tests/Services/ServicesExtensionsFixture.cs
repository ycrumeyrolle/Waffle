namespace Waffle.Tests.Services
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Waffle;
    using Waffle.Commands;
    using Waffle.Services;
    using Waffle.Tests.Helpers;

    [TestClass]
    public class ServicesExtensionsFixture
    {
        [TestMethod]
        public void WhenGettingOrThrowUnknowServiceThenThrowsInvalidOperationException()
        {
            ProcessorConfiguration config = new ProcessorConfiguration();
            DefaultServices services = new DefaultServices(config);
            services.Replace(typeof(ICommandHandlerActivator), null);

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => services.GetServiceOrThrow<ICommandHandlerActivator>());
        }
    }
}