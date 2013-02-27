namespace CommandProcessing.Tests.Helpers
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class ExceptionAssert
    {
        internal static TException Throws<TException>(Action action) where TException : Exception
        {
            TException exception = null;
            try
            {
                action();
            }
            catch (Exception e)
            {
                exception = e as TException;
            }

            Assert.IsNotNull(exception);
            return exception;
        }

        internal static void ThrowsArgumentNull(Action action, string paramName)
        {
            ArgumentNullException exception = Throws<ArgumentNullException>(action);
            Assert.AreEqual(paramName, exception.ParamName);
        }
    }
}
