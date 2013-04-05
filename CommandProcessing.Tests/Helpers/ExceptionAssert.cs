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
                exception = UnwrapException(e) as TException;
            }

            Assert.IsNotNull(exception);
            return exception;
        }

        private static Exception UnwrapException(Exception exception)
        {
            AggregateException aggEx;
            while ((aggEx = exception as AggregateException) != null)
            {
                exception = aggEx.GetBaseException();
            }

            return exception;
        }


        internal static void ThrowsArgumentNull(Action action, string paramName)
        {
            ArgumentNullException exception = Throws<ArgumentNullException>(action);
            Assert.AreEqual(paramName, exception.ParamName);
        }

        internal static void ThrowsArgument(Action action, string paramName)
        {
            ArgumentException exception = Throws<ArgumentException>(action);
            Assert.AreEqual(paramName, exception.ParamName);
        }    
        
        /// <summary>
        /// Verifies that the code throws an ArgumentOutOfRangeException (or optionally any exception which derives from it).
        /// </summary>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <param name="paramName">The name of the parameter that should throw the exception</param>
          /// <returns>The exception that was thrown, when successful</returns>
        internal static ArgumentOutOfRangeException ThrowsArgumentOutOfRange(Action testCode, string paramName)
        {
            var ex = Throws<ArgumentOutOfRangeException>(testCode);

            if (paramName != null)
            {
                Assert.AreEqual(paramName, ex.ParamName);
            }

            return ex;
        }
    }
}
