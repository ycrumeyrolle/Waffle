namespace Waffle.Tests.Helpers
{
    using System;
    using Xunit;

    public static class ExceptionAssert
    {
        private static Exception UnwrapException(Exception exception)
        {
            AggregateException aggEx;
            while ((aggEx = exception as AggregateException) != null)
            {
                exception = aggEx.GetBaseException();
            }

            return exception;
        }

        public static void ThrowsArgumentNull(Action action, string paramName)
        {
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal(paramName, exception.ParamName);
        }

        public static void ThrowsArgument(Action action, string paramName)
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal(paramName, exception.ParamName);
        }    
        
        /// <summary>
        /// Verifies that the code throws an ArgumentOutOfRangeException (or optionally any exception which derives from it).
        /// </summary>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <param name="paramName">The name of the parameter that should throw the exception</param>
          /// <returns>The exception that was thrown, when successful</returns>
        public static ArgumentOutOfRangeException ThrowsArgumentOutOfRange(Action testCode, string paramName)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(testCode);

            if (paramName != null)
            {
                Assert.Equal(paramName, ex.ParamName);
            }

            return ex;
        }
    }
}
