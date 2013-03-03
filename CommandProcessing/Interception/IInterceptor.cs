namespace CommandProcessing.Interception
{
    using System;
    
    /// <summary>
    /// Defines the methods that are required to intercept method.
    /// </summary>
    public interface IInterceptor
    {
        void OnExecuting();

        void OnExecuted();

        void OnException(Exception exception);
    }
}
