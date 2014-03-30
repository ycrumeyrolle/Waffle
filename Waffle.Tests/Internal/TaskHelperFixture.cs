namespace Waffle.Tests.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using Moq;
    using Waffle.Tasks;
    using Waffle.Tests.Helpers;

    
    public class TaskHelperFixture
    {
        // -----------------------------------------------------------------
        //  TaskHelpers.Canceled

        [Fact]
        public void Canceled_ReturnsCanceledTask()
        {
            Task result = TaskHelpers.Canceled();

            Assert.NotNull(result);
            Assert.True(result.IsCanceled);
        }

        // -----------------------------------------------------------------
        //  TaskHelpers.Canceled<T>

        [Fact]
        public void Canceled_Generic_ReturnsCanceledTask()
        {
            Task<string> result = TaskHelpers.Canceled<string>();

            Assert.NotNull(result);
            Assert.True(result.IsCanceled);
        }

        // -----------------------------------------------------------------
        //  TaskHelpers.Completed

        [Fact]
        public void Completed_ReturnsCompletedTask()
        {
            Task result = TaskHelpers.Completed();

            Assert.NotNull(result);
            Assert.Equal(TaskStatus.RanToCompletion, result.Status);
        }

        // -----------------------------------------------------------------
        //  TaskHelpers.FromError

        [Fact]
        public void FromError_ReturnsFaultedTaskWithGivenException()
        {
            var exception = new Exception();

            Task result = TaskHelpers.FromError(exception);

            Assert.NotNull(result);
            Assert.True(result.IsFaulted);
            Assert.Same(exception, result.Exception.InnerException);
        }

        // -----------------------------------------------------------------
        //  TaskHelpers.FromError<T>

        [Fact]
        public void FromError_Generic_ReturnsFaultedTaskWithGivenException()
        {
            var exception = new Exception();

            Task<string> result = TaskHelpers.FromError<string>(exception);

            Assert.NotNull(result);
            Assert.True(result.IsFaulted);
            Assert.Same(exception, result.Exception.InnerException);
        }

        // -----------------------------------------------------------------
        //  Task.FromResult<T>

        [Fact]
        public void FromResult_ReturnsCompletedTaskWithGivenResult()
        {
            string s = "ABC";

            Task<string> result = Task.FromResult(s);

            Assert.NotNull(result);
            Assert.True(result.Status == TaskStatus.RanToCompletion);
            Assert.Same(s, result.Result);
        }
   
        // ----------------------------------------------------------------
        //   Task<object> Task<T>.CastToObject()

        [Fact]
        public void ConvertFromTaskOfStringShouldSucceed()
        {
            // Arrange
            Task.FromResult("StringResult")

            // Act
                .CastToObject()

            // Assert
                .ContinueWith(task =>
                {
                    Assert.Equal(TaskStatus.RanToCompletion, task.Status);
                    Assert.Equal("StringResult", (string)task.Result);
                });
        }

        [Fact]
        public void ConvertFromTaskOfIntShouldSucceed()
        {
            // Arrange
            Task.FromResult(123)

            // Act
                .CastToObject()

            // Assert
                .ContinueWith(task =>
                {
                    Assert.Equal(TaskStatus.RanToCompletion, task.Status);
                    Assert.Equal(123, (int)task.Result);
                });
        }

        [Fact]
        public void ConvertFromFaultedTaskOfObjectShouldBeHandled()
        {
            // Arrange
            TaskHelpers.FromError<object>(new InvalidOperationException())

            // Act
                .CastToObject()

            // Assert
                .ContinueWith(task =>
                {
                    Assert.Equal(TaskStatus.Faulted, task.Status);
                    Assert.IsType(typeof(InvalidOperationException), task.Exception.GetBaseException());
                });
        }

        [Fact]
        public void ConvertFromCancelledTaskOfStringShouldBeHandled()
        {
            // Arrange
            TaskHelpers.Canceled<string>()

            // Act
                .CastToObject()

            // Assert
                .ContinueWith(task =>
                {
                    Assert.Equal(TaskStatus.Canceled, task.Status);
                });
        }

        // ----------------------------------------------------------------
        //   Task<object> Task.CastToObject()

        [Fact]
        public void ConvertFromTaskShouldSucceed()
        {
            // Arrange
            TaskHelpers.Completed()

            // Act
                .CastToObject()

            // Assert
                .ContinueWith(task =>
                {
                    Assert.Equal(TaskStatus.RanToCompletion, task.Status);
                    Assert.Equal(null, task.Result);
                });
        }

        [Fact]
        public void ConvertFromFaultedTaskShouldBeHandled()
        {
            // Arrange
            TaskHelpers.FromError(new InvalidOperationException())

            // Act
                .CastToObject()

            // Assert
                .ContinueWith(task =>
                {
                    Assert.Equal(TaskStatus.Faulted, task.Status);
                    Assert.IsType(typeof(InvalidOperationException), task.Exception.GetBaseException());
                });
        }

        [Fact]
        public void ConvertFromCancelledTaskShouldBeHandled()
        {
            // Arrange
            TaskHelpers.Canceled()

            // Act
                .CastToObject()

            // Assert
                .ContinueWith(task =>
                {
                    Assert.Equal(TaskStatus.Canceled, task.Status);
                });
        }

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