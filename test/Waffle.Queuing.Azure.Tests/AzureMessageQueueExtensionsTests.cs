namespace Waffle.Queuing.Azure.Tests
{
    using System;
    using Xunit;

    public class AzureMessageQueueExtensionsTests
    {
        [Fact]
        public void EnableAzureMessageQueuing()
        {
            // Arrange 
            ProcessorConfiguration config = new ProcessorConfiguration();

            // Assert
            config.EnableAzureMessageQueuing("Endpoint=sb://test.servicebus.windows.net;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SECRET");

            // Assert
            Assert.IsType<AzureCommandQueue>(config.Services.GetService<ICommandReceiver>());
            Assert.IsType<AzureCommandQueue>(config.Services.GetService<ICommandSender>());
        }

        [Fact]
        public void EnableAzureMessageQueuing_NullConfiguration_ThrowsArgumentNullException()
        {
            // Assert
            Assert.Throws<ArgumentNullException>(() => AzureMessageQueueExtensions.EnableAzureMessageQueuing(null, "Endpoint=sb://test.servicebus.windows.net;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SECRET"));
        }
    }
}
