namespace CommandProcessing.Interception
{
    using System;

    /// <summary>
    /// Defines the methods that are required to intercept a method.
    /// </summary>
    public interface IInterceptionProvider
    {
        void OnExecuting();

        void OnExecuted();

        void OnException(Exception exception);
    }
}
