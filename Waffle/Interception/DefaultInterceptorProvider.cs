namespace Waffle.Interception
{
    using System;
    using System.Collections.Generic;
    using Waffle.Internal;

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
            IInterceptor[] interceptors = this.configuration.Services.GetInterceptors();
            for (int index = 0; index < interceptors.Length; index++)
            {
                interceptors[index].OnExecuting();
            }
        }

        /// <summary>
        /// Occurs after the service method is invoked.
        /// </summary>
        public void OnExecuted()
        {
            IInterceptor[] interceptors = this.configuration.Services.GetInterceptors();
            for (int index = 0; index < interceptors.Length; index++)
            {
                interceptors[index].OnExecuted();
            }
        }

        /// <summary>
        /// Occurs when the service method is raise an exception.
        /// </summary>
        /// <param name="exception">The raised <see cref="Exception"/></param>
        public void OnException(Exception exception)
        {
            IInterceptor[] interceptors = this.configuration.Services.GetInterceptors();
            for (int index = 0; index < interceptors.Length; index++)
            {
                interceptors[index].OnException(exception);
            }
        }
    }
}
