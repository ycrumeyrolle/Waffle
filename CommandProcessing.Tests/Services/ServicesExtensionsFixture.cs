namespace CommandProcessing.Tests.Services
{
    using System;
    using CommandProcessing;
    using CommandProcessing.Dispatcher;
    using CommandProcessing.Services;
    using CommandProcessing.Tests.Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ServicesExtensionsFixture
    {
        [TestMethod]
        public void WhenGettingOrThrowUnknowServiceThenThrowsInvalidOperationException()
        {
            ProcessorConfiguration config = new ProcessorConfiguration();
            DefaultServices services = new DefaultServices(config);
            services.Replace(typeof(IHandlerActivator), null);

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => services.GetServiceOrThrow<IHandlerActivator>());
        }
    }
}