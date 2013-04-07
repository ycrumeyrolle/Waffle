namespace CommandProcessing.Interception
{
    using System;

    /// <summary>
    /// Defines the methods that are required to manage <see cref="IInterceptor"/>.
    /// </summary>
    public interface IInterceptionProvider
    {
        /// <summary>
        /// Occurs before the service method is invoked.
        /// </summary>
        void OnExecuting();

        /// <summary>
        /// Occurs after the service method is invoked.
        /// </summary>
        void OnExecuted();

        /// <summary>
        /// Occurs when the service method is raise an exception.
        /// </summary>
        /// <param name="exception">The raised <see cref="Exception"/></param>
        void OnException(Exception exception);
    }
}
