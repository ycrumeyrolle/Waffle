namespace CommandProcessing.Interception
{
    using System;
    using System.Collections.Generic;
    using CommandProcessing.Internal;
    using CommandProcessing.Services;

    /// <summary>
    /// Provides a default implementation of the <see cref="IInterceptionProvider"/> service.
    /// </summary>
    public class DefaultInterceptionProvider : IInterceptionProvider
    {
        private readonly ProcessorConfiguration configuration;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultInterceptionProvider"/> class.
        /// </summary>
        public DefaultInterceptionProvider(ProcessorConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            this.configuration = configuration;
        }

        /// <summary>
        /// Occurs before the service method is invoked.
        /// </summary>
        public void OnExecuting()
        {
            IEnumerable<IInterceptor> interceptors = this.configuration.Services.GetInterceptors();
            foreach (IInterceptor interceptor in interceptors)
            {
                interceptor.OnExecuting();
            }
        }

        /// <summary>
        /// Occurs after the service method is invoked.
        /// </summary>
        public void OnExecuted()
        {
            IEnumerable<IInterceptor> interceptors = this.configuration.Services.GetInterceptors();
            foreach (IInterceptor interceptor in interceptors)
            {
                interceptor.OnExecuted();
            }
        }

        /// <summary>
        /// Occurs when the service method is raise an exception.
        /// </summary>
        /// <param name="exception">The raised <see cref="Exception"/></param>
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
