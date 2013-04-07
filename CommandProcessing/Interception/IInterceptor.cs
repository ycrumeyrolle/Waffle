namespace CommandProcessing.Interception
{
    using System;
    
    /// <summary>
    /// Defines the methods that are required to intercept method.
    /// </summary>
    public interface IInterceptor
    {
        /// <summary>
        /// Occurs before a service method is invoked.
        /// </summary>
        void OnExecuting();

        /// <summary>
        /// Occurs after a service method is invoked.
        /// </summary>
        void OnExecuted();

        /// <summary>
        /// Occurs when a service method is raise an exception.
        /// </summary>
        /// <param name="exception">The raised <see cref="Exception"/></param>
        void OnException(Exception exception);
    }
}
