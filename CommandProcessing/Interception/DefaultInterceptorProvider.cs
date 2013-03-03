namespace CommandProcessing.Interception
{
    using System;
    using CommandProcessing.Services;

    public class DefaultInterceptionProvider : IInterceptionProvider
    {
        private readonly ProcessorConfiguration configuration;

        public DefaultInterceptionProvider(ProcessorConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void OnExecuting()
        {
            var interceptors = this.configuration.Services.GetInterceptors();
            foreach (var interceptor in interceptors)
            {
                interceptor.OnExecuting();
            }
        }

        public void OnExecuted()
        {
            var interceptors = this.configuration.Services.GetInterceptors();
            foreach (var interceptor in interceptors)
            {
                interceptor.OnExecuted();
            }
        }

        public void OnException(Exception exception)
        {
            var interceptors = this.configuration.Services.GetInterceptors();
            foreach (var interceptor in interceptors)
            {
                interceptor.OnException(exception);
            }
        }
    }
}
