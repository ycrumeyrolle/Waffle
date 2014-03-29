namespace Waffle.Tests.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Waffle.Tasks;
    using Waffle.Tests.Helpers;

    [TestClass]
    public class TaskHelperFixture
    {
        // -----------------------------------------------------------------
        //  TaskHelpers.Canceled

        [TestMethod]
        public void Canceled_ReturnsCanceledTask()
        {
            Task result = TaskHelpers.Canceled();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsCanceled);
        }

        // -----------------------------------------------------------------
        //  TaskHelpers.Canceled<T>

        [TestMethod]
        public void Canceled_Generic_ReturnsCanceledTask()
        {
            Task<string> result = TaskHelpers.Canceled<string>();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsCanceled);
        }

        // -----------------------------------------------------------------
        //  TaskHelpers.Completed

        [TestMethod]
        public void Completed_ReturnsCompletedTask()
        {
            Task result = TaskHelpers.Completed();

            Assert.IsNotNull(result);
            Assert.AreEqual(TaskStatus.RanToCompletion, result.Status);
        }

        // -----------------------------------------------------------------
        //  TaskHelpers.FromError

        [TestMethod]
        public void FromError_ReturnsFaultedTaskWithGivenException()
        {
            var exception = new Exception();

            Task result = TaskHelpers.FromError(exception);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsFaulted);
            Assert.AreSame(exception, result.Exception.InnerException);
        }

        // -----------------------------------------------------------------
        //  TaskHelpers.FromError<T>

        [TestMethod]
        public void FromError_Generic_ReturnsFaultedTaskWithGivenException()
        {
            var exception = new Exception();

            Task<string> result = TaskHelpers.FromError<string>(exception);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsFaulted);
            Assert.AreSame(exception, result.Exception.InnerException);
        }

        // -----------------------------------------------------------------
        //  Task.FromResult<T>

        [TestMethod]
        public void FromResult_ReturnsCompletedTaskWithGivenResult()
        {
            string s = "ABC";

            Task<string> result = Task.FromResult(s);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Status == TaskStatus.RanToCompletion);
            Assert.AreSame(s, result.Result);
        }
   
        // ----------------------------------------------------------------
        //   Task<object> Task<T>.CastToObject()

        [TestMethod]
        public void ConvertFromTaskOfStringShouldSucceed()
        {
            // Arrange
            Task.FromResult("StringResult")

            // Act
                .CastToObject()

            // Assert
                .ContinueWith(task =>
                {
                    Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
                    Assert.AreEqual("StringResult", (string)task.Result);
                });
        }

        [TestMethod]
        public void ConvertFromTaskOfIntShouldSucceed()
        {
            // Arrange
            Task.FromResult(123)

            // Act
                .CastToObject()

            // Assert
                .ContinueWith(task =>
                {
                    Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
                    Assert.AreEqual(123, (int)task.Result);
                });
        }

        [TestMethod]
        public void ConvertFromFaultedTaskOfObjectShouldBeHandled()
        {
            // Arrange
            TaskHelpers.FromError<object>(new InvalidOperationException())

            // Act
                .CastToObject()

            // Assert
                .ContinueWith(task =>
                {
                    Assert.AreEqual(TaskStatus.Faulted, task.Status);
                    Assert.IsInstanceOfType(task.Exception.GetBaseException(), typeof(InvalidOperationException));
                });
        }

        [TestMethod]
        public void ConvertFromCancelledTaskOfStringShouldBeHandled()
        {
            // Arrange
            TaskHelpers.Canceled<string>()

            // Act
                .CastToObject()

            // Assert
                .ContinueWith(task =>
                {
                    Assert.AreEqual(TaskStatus.Canceled, task.Status);
                });
        }

        // ----------------------------------------------------------------
        //   Task<object> Task.CastToObject()

        [TestMethod]
        public void ConvertFromTaskShouldSucceed()
        {
            // Arrange
            TaskHelpers.Completed()

            // Act
                .CastToObject()

            // Assert
                .ContinueWith(task =>
                {
                    Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
                    Assert.AreEqual(null, task.Result);
                });
        }

        [TestMethod]
        public void ConvertFromFaultedTaskShouldBeHandled()
        {
            // Arrange
            TaskHelpers.FromError(new InvalidOperationException())

            // Act
                .CastToObject()

            // Assert
                .ContinueWith(task =>
                {
                    Assert.AreEqual(TaskStatus.Faulted, task.Status);
                    Assert.IsInstanceOfType(task.Exception.GetBaseException(), typeof(InvalidOperationException));
                });
        }

        [TestMethod]
        public void ConvertFromCancelledTaskShouldBeHandled()
        {
            // Arrange
            TaskHelpers.Canceled()

            // Act
                .CastToObject()

            // Assert
                .ContinueWith(task =>
                {
                    Assert.AreEqual(TaskStatus.Canceled, task.Status);
                });
        }

        [TestCleanup]
        public void ForceGC()
        {
            GC.Collect(99);
            GC.Collect(99);
            GC.Collect(99);
        }

        private class ThreadContextPreserver : IDisposable
        {
            private readonly SynchronizationContext syncContext;

            public ThreadContextPreserver()
            {
                this.syncContext = SynchronizationContext.Current;
            }

            public void Dispose()
            {
                SynchronizationContext.SetSynchronizationContext(this.syncContext);
            }
        }
    }
}