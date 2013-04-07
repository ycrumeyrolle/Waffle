namespace CommandProcessing.Interception
{
    using System;
    using System.Collections.Generic;
    using CommandProcessing.Internal;
    using CommandProcessing.Services;

    public class DefaultInterceptionProvider : IInterceptionProvider
    {
        private readonly ProcessorConfiguration configuration;

        public DefaultInterceptionProvider(ProcessorConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            this.configuration = configuration;
        }

        public void OnExecuting()
        {
            IEnumerable<IInterceptor> interceptors = this.configuration.Services.GetInterceptors();
            foreach (var interceptor in interceptors)
            {
                interceptor.OnExecuting();
            }
        }

        public void OnExecuted()
        {
            IEnumerable<IInterceptor> interceptors = this.configuration.Services.GetInterceptors();
            foreach (var interceptor in interceptors)
            {
                interceptor.OnExecuted();
            }
        }

        public void OnException(Exception exception)
        {
            IEnumerable<IInterceptor> interceptors = this.configuration.Services.GetInterceptors();
            foreach (var interceptor in interceptors)
            {
                interceptor.OnException(exception);
            }
        }
    }
}
