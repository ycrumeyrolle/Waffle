namespace CommandProcessing.Tests.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CommandProcessing.Tasks;
    using CommandProcessing.Tests.Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

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
        //  TaskHelpers.FromErrors

        [TestMethod]
        public void FromErrors_ReturnsFaultedTaskWithGivenExceptions()
        {
            var exceptions = new[] { new Exception(), new InvalidOperationException() };

            Task result = TaskHelpers.FromErrors(exceptions);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsFaulted);
            Assert.IsNotNull(result.Exception);
            CollectionAssert.AreEqual(exceptions, result.Exception.InnerExceptions.ToArray());
        }

        // -----------------------------------------------------------------
        //  TaskHelpers.FromErrors<T>

        [TestMethod]
        public void FromErrors_Generic_ReturnsFaultedTaskWithGivenExceptions()
        {
            var exceptions = new[] { new Exception(), new InvalidOperationException() };

            Task<string> result = TaskHelpers.FromErrors<string>(exceptions);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsFaulted);
            Assert.IsNotNull(result.Exception);
            CollectionAssert.AreEqual(exceptions, result.Exception.InnerExceptions.ToArray());
        }

        // -----------------------------------------------------------------
        //  TaskHelpers.FromResult<T>

        [TestMethod]
        public void FromResult_ReturnsCompletedTaskWithGivenResult()
        {
            string s = "ABC";

            Task<string> result = TaskHelpers.FromResult(s);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Status == TaskStatus.RanToCompletion);
            Assert.AreSame(s, result.Result);
        }

        // -----------------------------------------------------------------
        //  Task TaskHelpers.Iterate(IEnumerable<Task>)

        [TestMethod]
        public void Iterate_NonGeneric_IfProvidedEnumerationContainsNullValue_ReturnsFaultedTaskWithNullReferenceException()
        {
            List<string> log = new List<string>();

            var result = TaskHelpers.Iterate(NullTaskEnumerable(log));

            Assert.IsNotNull(result);
            result.WaitUntilCompleted();
            Assert.AreEqual(TaskStatus.Faulted, result.Status);
            Assert.IsInstanceOfType(result.Exception.GetBaseException(), typeof(NullReferenceException));
        }

        private static IEnumerable<Task> NullTaskEnumerable(List<string> log)
        {
            log.Add("first");
            yield return null;
            log.Add("second");
        }

        [TestMethod]
        public void Iterate_NonGeneric_IfProvidedEnumerationThrowsException_ReturnsFaultedTask()
        {
            List<string> log = new List<string>();
            Exception exception = new Exception();

            var result = TaskHelpers.Iterate(ThrowingTaskEnumerable(exception, log));

            Assert.IsNotNull(result);
            result.WaitUntilCompleted();
            Assert.AreEqual(TaskStatus.Faulted, result.Status);
            Assert.IsNotNull(result.Exception);
            Assert.AreSame(exception, result.Exception.InnerException);
            CollectionAssert.AreEqual(new[] { "first" }, log.ToArray());
        }

        private static IEnumerable<Task> ThrowingTaskEnumerable(Exception e, List<string> log)
        {
            log.Add("first");
            bool a = 1 + 1 == 2; // work around unreachable code warning
            if (a)
            {
                throw e;
            }

            log.Add("second");
            yield return null;
        }

        [TestMethod]
        public void Iterate_NonGeneric_IfProvidedEnumerableExecutesCancellingTask_ReturnsCanceledTaskAndHaltsEnumeration()
        {
            List<string> log = new List<string>();

            var result = TaskHelpers.Iterate(CanceledTaskEnumerable(log));

            Assert.IsNotNull(result);
            result.WaitUntilCompleted();
            Assert.AreEqual(TaskStatus.Canceled, result.Status);
            CollectionAssert.AreEqual(new[] { "first" }, log.ToArray());
        }

        private static IEnumerable<Task> CanceledTaskEnumerable(List<string> log)
        {
            log.Add("first");
            yield return TaskHelpers.Canceled();
            log.Add("second");
        }

        [TestMethod]
        public void Iterate_NonGeneric_IfProvidedEnumerableExecutesFaultingTask_ReturnsCanceledTaskAndHaltsEnumeration()
        {
            List<string> log = new List<string>();
            Exception exception = new Exception();

            var result = TaskHelpers.Iterate(FaultedTaskEnumerable(exception, log));

            Assert.IsNotNull(result);
            result.WaitUntilCompleted();
            Assert.AreEqual(TaskStatus.Faulted, result.Status);
            Assert.IsNotNull(result.Exception);
            Assert.AreSame(exception, result.Exception.InnerException);
            CollectionAssert.AreEqual(new[] { "first" }, log.ToArray());
        }

        private static IEnumerable<Task> FaultedTaskEnumerable(Exception e, List<string> log)
        {
            log.Add("first");
            yield return TaskHelpers.FromError(e);
            log.Add("second");
        }

        [TestMethod]
        public void Iterate_NonGeneric_ExecutesNextTaskOnlyAfterPreviousTaskSucceeded()
        {
            List<string> log = new List<string>();

            var result = TaskHelpers.Iterate(SuccessTaskEnumerable(log));

            Assert.IsNotNull(result);
            result.WaitUntilCompleted();
            Assert.AreEqual(TaskStatus.RanToCompletion, result.Status);
            CollectionAssert.AreEqual(
                new[] { "first", "Executing first task. Log size: 1", "second", "Executing second task. Log size: 3" },
                log.ToArray());
        }

        private static IEnumerable<Task> SuccessTaskEnumerable(List<string> log)
        {
            log.Add("first");
            yield return Task.Factory.StartNew(() => log.Add("Executing first task. Log size: " + log.Count));
            log.Add("second");
            yield return Task.Factory.StartNew(() => log.Add("Executing second task. Log size: " + log.Count));
        }

        [TestMethod]
        public void Iterate_NonGeneric_TasksRunSequentiallyRegardlessOfExecutionTime()
        {
            List<string> log = new List<string>();

            Task task = TaskHelpers.Iterate(TasksWithVaryingDelays(log, 100, 1, 50, 2));

            task.WaitUntilCompleted();
            Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
            CollectionAssert.AreEqual(new[] { "ENTER: 100", "EXIT: 100", "ENTER: 1", "EXIT: 1", "ENTER: 50", "EXIT: 50", "ENTER: 2", "EXIT: 2" }, log);
        }

        private static IEnumerable<Task> TasksWithVaryingDelays(List<string> log, params int[] delays)
        {
            foreach (int delay in delays)
                yield return Task.Factory.StartNew(timeToSleep =>
                    {
                        log.Add("ENTER: " + timeToSleep);
                        Thread.Sleep((int)timeToSleep);
                        log.Add("EXIT: " + timeToSleep);
                    }, delay);
        }

        [TestMethod]
        public void Iterate_NonGeneric_StopsTaskIterationIfCancellationWasRequested()
        {
            List<string> log = new List<string>();
            CancellationTokenSource cts = new CancellationTokenSource();

            var result = TaskHelpers.Iterate(CancelingTaskEnumerable(log, cts), cts.Token);

            Assert.IsNotNull(result);
            result.WaitUntilCompleted();
            Assert.AreEqual(TaskStatus.Canceled, result.Status);
            CollectionAssert.AreEqual(
                new[] { "first", "Executing first task. Log size: 1" },
                log.ToArray());
        }

        private static IEnumerable<Task> CancelingTaskEnumerable(List<string> log, CancellationTokenSource cts)
        {
            log.Add("first");
            yield return Task.Factory.StartNew(() =>
                {
                    log.Add("Executing first task. Log size: " + log.Count);
                    cts.Cancel();
                });
            log.Add("second");
            yield return Task.Factory.StartNew(() =>
                {
                    log.Add("Executing second task. Log size: " + log.Count);
                });
        }

        [TestMethod]
        public void Iterate_NonGeneric_IteratorRunsInSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                ThreadPoolSyncContext sc = new ThreadPoolSyncContext();
                SynchronizationContext.SetSynchronizationContext(sc);

                TaskHelpers.Iterate(SyncContextVerifyingEnumerable(sc)).Then(() =>
                    {
                        Assert.AreSame(sc, SynchronizationContext.Current);
                    });
            }
        }

        private static IEnumerable<Task> SyncContextVerifyingEnumerable(SynchronizationContext sc)
        {
            for (int i = 0; i < 10; i++)
            {
                Assert.AreSame(sc, SynchronizationContext.Current);
                yield return TaskHelpers.Completed();
            }
        }

        // -----------------------------------------------------------------
        //  TaskHelpers.TrySetFromTask<T>

        [TestMethod]
        public void TrySetFromTask_IfSourceTaskIsCanceled_CancelsTaskCompletionSource()
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            Task canceledTask = TaskHelpers.Canceled<object>();

            tcs.TrySetFromTask(canceledTask);

            Assert.AreEqual(TaskStatus.Canceled, tcs.Task.Status);
        }

        [TestMethod]
        public void TrySetFromTask_IfSourceTaskIsFaulted_FaultsTaskCompletionSource()
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            Exception exception = new Exception();
            Task faultedTask = TaskHelpers.FromError<object>(exception);

            tcs.TrySetFromTask(faultedTask);

            Assert.AreEqual(TaskStatus.Faulted, tcs.Task.Status);
            Assert.AreSame(exception, tcs.Task.Exception.InnerException);
        }

        [TestMethod]
        public void TrySetFromTask_IfSourceTaskIsSuccessfulAndOfSameResultType_SucceedsTaskCompletionSourceAndSetsResult()
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            Task<string> successfulTask = TaskHelpers.FromResult("abc");

            tcs.TrySetFromTask(successfulTask);

            Assert.AreEqual(TaskStatus.RanToCompletion, tcs.Task.Status);
            Assert.AreEqual("abc", tcs.Task.Result);
        }

        [TestMethod]
        public void TrySetFromTask_IfSourceTaskIsSuccessfulAndOfDifferentResultType_SucceedsTaskCompletionSourceAndSetsDefaultValueAsResult()
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            Task<object> successfulTask = TaskHelpers.FromResult(new object());

            tcs.TrySetFromTask(successfulTask);

            Assert.AreEqual(TaskStatus.RanToCompletion, tcs.Task.Status);
            Assert.AreEqual(null, tcs.Task.Result);
        }

        // ----------------------------------------------------------------
        //   Task<T> Task<object>.CastFromObject()

        [TestMethod]
        public void ConvertToTaskOfStringShouldSucceed()
        {
            // Arrange
            TaskHelpers.FromResult((object)"StringResult")

            // Act
                .CastFromObject<string>()

            // Assert
                .ContinueWith((task) =>
                {
                    Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
                    Assert.AreEqual("StringResult", task.Result);
                });
        }

        [TestMethod]
        public void ConvertToTaskOfIntShouldSucceed()
        {
            // Arrange
            TaskHelpers.FromResult((object)123)

            // Act
                .CastFromObject<int>()

            // Assert
                .ContinueWith((task) =>
                {
                    Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
                    Assert.AreEqual(123, task.Result);
                });
        }

        [TestMethod]
        public void ConvertToTaskOfWrongTypeShouldFail()
        {
            // Arrange
            TaskHelpers.FromResult((object)123)

            // Act
                .CastFromObject<string>()

            // Assert
                .ContinueWith((task) =>
                {
                    Assert.AreEqual(TaskStatus.Faulted, task.Status);
                    Assert.IsInstanceOfType(task.Exception.GetBaseException(), typeof(InvalidCastException));
                });
        }

        [TestMethod]
        public void ConvertFromFaultedTaskOfObjectToTaskOfStringShouldBeHandled()
        {
            // Arrange
            TaskHelpers.FromError<object>(new InvalidOperationException())

            // Act
                .CastFromObject<string>()

            // Assert
                .ContinueWith((task) =>
                {
                    Assert.AreEqual(TaskStatus.Faulted, task.Status);;
                    Assert.IsInstanceOfType(task.Exception.GetBaseException(), typeof(InvalidOperationException));
                });
        }

        [TestMethod]
        public void ConvertFromFaultedTaskOfObjectToTaskOfIntShouldBeHandled()
        {
            // Arrange
            TaskHelpers.FromError<object>(new InvalidOperationException())

            // Act
                .CastFromObject<int>()

            // Assert
                .ContinueWith((task) =>
                {
                    Assert.AreEqual(TaskStatus.Faulted, task.Status);
                    Assert.IsInstanceOfType(task.Exception.GetBaseException(), typeof(InvalidOperationException));
                });
        }

        [TestMethod]
        public void ConvertToCancelledTaskOfStringShouldBeHandled()
        {
            // Arrange
            TaskHelpers.Canceled<object>()

            // Act
                .CastFromObject<string>()

            // Assert
                .ContinueWith((task) =>
                {
                    Assert.AreEqual(TaskStatus.Canceled, task.Status);
                });
        }

        [TestMethod]
        public void ConvertToCancelledTaskOfIntShouldBeHandled()
        {
            // Arrange
            TaskHelpers.Canceled<object>()

            // Act
                .CastFromObject<int>()

            // Assert
                .ContinueWith((task) =>
                {
                    Assert.AreEqual(TaskStatus.Canceled, task.Status);
                });
        }

        // ----------------------------------------------------------------
        //   Task<object> Task<T>.CastToObject()

        [TestMethod]
        public void ConvertFromTaskOfStringShouldSucceed()
        {
            // Arrange
            TaskHelpers.FromResult("StringResult")

            // Act
                .CastToObject()

            // Assert
                .ContinueWith((task) =>
                {
                    Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
                    Assert.AreEqual("StringResult", (string)task.Result);
                });
        }

        [TestMethod]
        public void ConvertFromTaskOfIntShouldSucceed()
        {
            // Arrange
            TaskHelpers.FromResult(123)

            // Act
                .CastToObject()

            // Assert
                .ContinueWith((task) =>
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
                .ContinueWith((task) =>
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
                .ContinueWith((task) =>
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
                .ContinueWith((task) =>
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
                .ContinueWith((task) =>
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
                .ContinueWith((task) =>
                {
                    Assert.AreEqual(TaskStatus.Canceled, task.Status);
                });
        }

        // -----------------------------------------------------------------
        //  Task.Catch(Func<Exception, Task>)

        [TestMethod]
        public void Catch_NoInputValue_CatchesException_Handled()
        {
            // Arrange
            TaskHelpers.FromError(new InvalidOperationException())

            // Act
                              .Catch(info =>
                              {
                                  Assert.IsNotNull(info.Exception);
                                  Assert.IsInstanceOfType(info.Exception, typeof(InvalidOperationException));
                                  return info.Handled();
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
                              });
        }

        [TestMethod]
        public void Catch_NoInputValue_CatchesException_Rethrow()
        {
            // Arrange
            TaskHelpers.FromError(new InvalidOperationException())

            // Act
                              .Catch(info =>
                              {
                                  return info.Throw();
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.Faulted, task.Status);
                                  Assert.IsInstanceOfType(task.Exception.GetBaseException(), typeof(InvalidOperationException));
                              });
        }

        [TestMethod]
        public void Catch_NoInputValue_ReturningEmptyCatchResultFromCatchIsProhibited()
        {
            // Arrange
            TaskHelpers.FromError(new Exception())

            // Act
                              .Catch(info =>
                              {
                                  return new CatchInfo.CatchResult();
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.Faulted, task.Status);
                                  Assert.IsInstanceOfType(task.Exception, typeof(InvalidOperationException), "You must set the Task property of the CatchInfo returned from the TaskHelpersExtensions.Catch continuation.");
                              });
        }

        [TestMethod]
        public void Catch_NoInputValue_CompletedTaskOfSuccess_DoesNotRunContinuationAndDoesNotSwitchContexts()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                bool ranContinuation = false;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                TaskHelpers.Completed()

                    // Act
                    .Catch(info =>
                        {
                            ranContinuation = true;
                            return info.Handled();
                        })

                    // Assert
                    .ContinueWith(task =>
                        {
                            Assert.IsFalse(ranContinuation);
                            syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Never());
                        });
            }
        }

        [TestMethod]
        public void Catch_NoInputValue_CompletedTaskOfCancellation_DoesNotRunContinuationAndDoesNotSwitchContexts()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                bool ranContinuation = false;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                TaskHelpers.Canceled()

                    // Act
                    .Catch(info =>
                        {
                            ranContinuation = true;
                            return info.Handled();
                        })

                    // Assert
                    .ContinueWith(task =>
                        {
                            Assert.IsFalse(ranContinuation);
                            syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Never());
                        });
            }
        }

        [TestMethod]
        public void Catch_NoInputValue_CompletedTaskOfFault_RunsOnSameThreadAndDoesNotPostToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int outerThreadId = Thread.CurrentThread.ManagedThreadId;
                int innerThreadId = Int32.MinValue;
                Exception thrownException = new Exception();
                Exception caughtException = null;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                TaskHelpers.FromError(thrownException)

                    // Act
                    .Catch(info =>
                        {
                            caughtException = info.Exception;
                            innerThreadId = Thread.CurrentThread.ManagedThreadId;
                            return info.Handled();
                        })

                    // Assert
                    .ContinueWith(task =>
                        {
                            Assert.AreSame(thrownException, caughtException);
                            Assert.AreEqual(innerThreadId, outerThreadId);
                            syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Never());
                        });
            }
        }

        [TestMethod]
        public void Catch_NoInputValue_IncompleteTaskOfSuccess_DoesNotRunContinuationAndDoesNotSwitchContexts()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                bool ranContinuation = false;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                Task incompleteTask = new Task(() => { });

                // Act
                Task resultTask = incompleteTask.Catch(info =>
                    {
                        ranContinuation = true;
                        return info.Handled();
                    });

                // Assert
                incompleteTask.Start();

                resultTask.ContinueWith(task =>
                    {
                        Assert.IsFalse(ranContinuation);
                        syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Never());
                    });
            }
        }

        [TestMethod]
        public void Catch_NoInputValue_IncompleteTaskOfCancellation_DoesNotRunContinuationAndDoesNotSwitchContexts()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                bool ranContinuation = false;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                Task incompleteTask = new Task(() => { });
                Task resultTask = incompleteTask.ContinueWith(task => TaskHelpers.Canceled()).Unwrap();

                // Act
                resultTask = resultTask.Catch(info =>
                    {
                        ranContinuation = true;
                        return info.Handled();
                    });

                // Assert
                incompleteTask.Start();

                resultTask.ContinueWith(task =>
                    {
                        Assert.IsFalse(ranContinuation);
                        syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Never());
                    });
            }
        }

        [TestMethod]
        public void Catch_NoInputValue_IncompleteTaskOfFault_RunsOnNewThreadAndPostsToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int outerThreadId = Thread.CurrentThread.ManagedThreadId;
                int innerThreadId = Int32.MinValue;
                Exception thrownException = new Exception();
                Exception caughtException = null;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                Task incompleteTask = new Task(() => { throw thrownException; });

                // Act
                Task resultTask = incompleteTask.Catch(info =>
                    {
                        caughtException = info.Exception;
                        innerThreadId = Thread.CurrentThread.ManagedThreadId;
                        return info.Handled();
                    });

                // Assert
                incompleteTask.Start();

                resultTask.ContinueWith(task =>
                    {
                        Assert.AreSame(thrownException, caughtException);
                        Assert.AreNotEqual(innerThreadId, outerThreadId);
                        syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Once());
                    });
            }
        }

        // -----------------------------------------------------------------
        //  Task<T>.Catch(Func<Exception, Task<T>>)

        [TestMethod]
        public void Catch_WithInputValue_CatchesException_Handled()
        {
            // Arrange
            TaskHelpers.FromError<int>(new InvalidOperationException())

            // Act
                              .Catch(info =>
                              {
                                  Assert.IsNotNull(info.Exception);
                                  Assert.IsInstanceOfType(info.Exception, typeof(InvalidOperationException));
                                  return info.Handled(42);
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
                              });
        }

        [TestMethod]
        public void Catch_WithInputValue_CatchesException_Rethrow()
        {
            // Arrange
            TaskHelpers.FromError<int>(new InvalidOperationException())

            // Act
                              .Catch(info =>
                              {
                                  return info.Throw();
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.Faulted, task.Status);
                                  Assert.IsInstanceOfType(task.Exception.GetBaseException(), typeof(InvalidOperationException));
                              });
        }

        [TestMethod]
        public void Catch_WithInputValue_ReturningNullFromCatchIsProhibited()
        {
            // Arrange
            TaskHelpers.FromError<int>(new Exception())

            // Act
                              .Catch(info =>
                              {
                                  return new CatchInfoBase<Task>.CatchResult();
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.Faulted, task.Status);
                                  Assert.IsInstanceOfType(task.Exception, typeof(InvalidOperationException), "You must set the Task property of the CatchInfo returned from the TaskHelpersExtensions.Catch continuation.");
                              });
        }

        [TestMethod]
        public void Catch_WithInputValue_CompletedTaskOfSuccess_DoesNotRunContinuationAndDoesNotSwitchContexts()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                bool ranContinuation = false;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                TaskHelpers.FromResult(21)

                    // Act
                    .Catch(info =>
                        {
                            ranContinuation = true;
                            return info.Handled(42);
                        })

                    // Assert
                    .ContinueWith(task =>
                        {
                            Assert.IsFalse(ranContinuation);
                            syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Never());
                        });
            }
        }

        [TestMethod]
        public void Catch_WithInputValue_CompletedTaskOfCancellation_DoesNotRunContinuationAndDoesNotSwitchContexts()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                bool ranContinuation = false;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                TaskHelpers.Canceled<int>()

                    // Act
                    .Catch(info =>
                        {
                            ranContinuation = true;
                            return info.Handled(42);
                        })

                    // Assert
                    .ContinueWith(task =>
                        {
                            Assert.IsFalse(ranContinuation);
                            syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Never());
                        });
            }
        }

        [TestMethod]
        public void Catch_WithInputValue_CompletedTaskOfFault_RunsOnSameThreadAndDoesNotPostToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int outerThreadId = Thread.CurrentThread.ManagedThreadId;
                int innerThreadId = Int32.MinValue;
                Exception thrownException = new Exception();
                Exception caughtException = null;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                TaskHelpers.FromError<int>(thrownException)

                    // Act
                    .Catch(info =>
                        {
                            caughtException = info.Exception;
                            innerThreadId = Thread.CurrentThread.ManagedThreadId;
                            return info.Handled(42);
                        })

                    // Assert
                    .ContinueWith(task =>
                        {
                            Assert.AreSame(thrownException, caughtException);
                            Assert.AreEqual(innerThreadId, outerThreadId);
                            syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Never());
                        });
            }
        }

        [TestMethod]
        public void Catch_WithInputValue_IncompleteTaskOfSuccess_DoesNotRunContinuationAndDoesNotSwitchContexts()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                bool ranContinuation = false;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                Task<int> incompleteTask = new Task<int>(() => 42);

                // Act
                Task<int> resultTask = incompleteTask.Catch(info =>
                    {
                        ranContinuation = true;
                        return info.Handled(42);
                    });

                // Assert
                incompleteTask.Start();

                resultTask.ContinueWith(task =>
                    {
                        Assert.IsFalse(ranContinuation);
                        syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Never());
                    });
            }
        }

        [TestMethod]
        public void Catch_WithInputValue_IncompleteTaskOfCancellation_DoesNotRunContinuationAndDoesNotSwitchContexts()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                bool ranContinuation = false;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                Task<int> incompleteTask = new Task<int>(() => 42);
                Task<int> resultTask = incompleteTask.ContinueWith(task => TaskHelpers.Canceled<int>()).Unwrap();

                // Act
                resultTask = resultTask.Catch(info =>
                    {
                        ranContinuation = true;
                        return info.Handled(2112);
                    });

                // Assert
                incompleteTask.Start();

                resultTask.ContinueWith(task =>
                    {
                        Assert.IsFalse(ranContinuation);
                        syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Never());
                    });
            }
        }

        [TestMethod]
        public void Catch_WithInputValue_IncompleteTaskOfFault_RunsOnNewThreadAndPostsToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int outerThreadId = Thread.CurrentThread.ManagedThreadId;
                int innerThreadId = Int32.MinValue;
                Exception thrownException = new Exception();
                Exception caughtException = null;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                Task<int> incompleteTask = new Task<int>(() => { throw thrownException; });

                // Act
                Task<int> resultTask = incompleteTask.Catch(info =>
                    {
                        caughtException = info.Exception;
                        innerThreadId = Thread.CurrentThread.ManagedThreadId;
                        return info.Handled(42);
                    });

                // Assert
                incompleteTask.Start();

                resultTask.ContinueWith(task =>
                    {
                        Assert.AreSame(thrownException, caughtException);
                        Assert.AreNotEqual(innerThreadId, outerThreadId);
                        syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Once());
                    });
            }
        }

        // -----------------------------------------------------------------
        //  Task.CopyResultToCompletionSource(Task)

        [TestMethod]
        public void CopyResultToCompletionSource_NoInputValue_SuccessfulTask()
        {
            // Arrange
            var tcs = new TaskCompletionSource<object>();
            var expectedResult = new object();

            TaskHelpers.Completed()

            // Act
                              .CopyResultToCompletionSource(tcs, expectedResult)

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.RanToCompletion, task.Status); // Outer task always runs to completion
                                  Assert.AreEqual(TaskStatus.RanToCompletion, tcs.Task.Status);
                                  Assert.AreSame(expectedResult, tcs.Task.Result);
                              });
        }

        [TestMethod]
        public void CopyResultToCompletionSource_NoInputValue_FaultedTask()
        {
            // Arrange
            var tcs = new TaskCompletionSource<object>();
            var expectedException = new NotImplementedException();

            TaskHelpers.FromError(expectedException)

            // Act
                              .CopyResultToCompletionSource(tcs, completionResult: null)

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.RanToCompletion, task.Status); // Outer task always runs to completion
                                  Assert.AreEqual(TaskStatus.Faulted, tcs.Task.Status);
                                  Assert.AreSame(expectedException, tcs.Task.Exception.GetBaseException());
                              });
        }

        [TestMethod]
        public void CopyResultToCompletionSource_NoInputValue_Canceled()
        {
            // Arrange
            var tcs = new TaskCompletionSource<object>();

            TaskHelpers.Canceled()

            // Act
                              .CopyResultToCompletionSource(tcs, completionResult: null)

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.RanToCompletion, task.Status); // Outer task always runs to completion
                                  Assert.AreEqual(TaskStatus.Canceled, tcs.Task.Status);
                              });
        }

        // -----------------------------------------------------------------
        //  Task.CopyResultToCompletionSource(Task<T>)

        [TestMethod]
        public void CopyResultToCompletionSource_WithInputValue_SuccessfulTask()
        {
            // Arrange
            var tcs = new TaskCompletionSource<int>();

            TaskHelpers.FromResult(42)

            // Act
                              .CopyResultToCompletionSource(tcs)

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.RanToCompletion, task.Status); // Outer task always runs to completion
                                  Assert.AreEqual(TaskStatus.RanToCompletion, tcs.Task.Status);
                                  Assert.AreEqual(42, tcs.Task.Result);
                              });
        }

        [TestMethod]
        public void CopyResultToCompletionSource_WithInputValue_FaultedTask()
        {
            // Arrange
            var tcs = new TaskCompletionSource<int>();
            var expectedException = new NotImplementedException();

            TaskHelpers.FromError<int>(expectedException)

            // Act
                              .CopyResultToCompletionSource(tcs)

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.RanToCompletion, task.Status); // Outer task always runs to completion
                                  Assert.AreEqual(TaskStatus.Faulted, tcs.Task.Status);
                                  Assert.AreSame(expectedException, tcs.Task.Exception.GetBaseException());
                              });
        }

        [TestMethod]
        public void CopyResultToCompletionSource_WithInputValue_Canceled()
        {
            // Arrange
            var tcs = new TaskCompletionSource<int>();

            TaskHelpers.Canceled<int>()

            // Act
                              .CopyResultToCompletionSource(tcs)

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.RanToCompletion, task.Status); // Outer task always runs to completion
                                  Assert.AreEqual(TaskStatus.Canceled, tcs.Task.Status);
                              });
        }

        // -----------------------------------------------------------------
        //  Task.Finally(Action)

        [TestMethod]
        public void Finally_NoInputValue_CompletedTaskOfSuccess_RunsOnSameThreadAndDoesNotPostToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int originalThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = Int32.MinValue;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                TaskHelpers.Completed()

                    // Act
                    .Finally(() =>
                        {
                            callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                        })

                    // Assert
                    .ContinueWith(task =>
                        {
                            Assert.AreEqual(originalThreadId, callbackThreadId);
                            syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Never());
                        });
            }
        }

        [TestMethod]
        public void Finally_CompletedTaskOfFault_ExceptionInFinally()
        {
            using (new ThreadContextPreserver())
            {
                Exception exception1 = new InvalidOperationException("From source");
                Exception exception2 = new InvalidOperationException("FromFinally");

                // When the finally clause throws, that's the exception that propagates. 
                // Still ensure that the original exception from the try block is observed.

                // Act 
                Task faultedTask = TaskHelpers.FromError(exception1);
                Task t = faultedTask.Finally(() => { throw exception2; });

                // Assert
                Assert.IsTrue(t.IsFaulted);
                Assert.IsInstanceOfType(t.Exception, typeof(AggregateException));
                Assert.AreEqual(1, t.Exception.InnerExceptions.Count);
                Assert.AreEqual(exception2, t.Exception.InnerException);
            }
        }

        [TestMethod]
        public void Finally_IncompletedTask_ExceptionInFinally()
        {
            using (new ThreadContextPreserver())
            {
                Exception exception1 = new InvalidOperationException("From source");
                Exception exception2 = new InvalidOperationException("FromFinally");

                // Like test Finally_CompletedTaskOfFault_ExceptionInFinally, but exercises when the original task doesn't complete synchronously

                // Act 
                Task incompleteTask = new Task(() => { throw exception1; });
                Task t = incompleteTask.Finally(() => { throw exception2; });

                incompleteTask.Start();

                // Assert
                t.ContinueWith(prevTask =>
                    {
                        Assert.AreEqual(t, prevTask);

                        Assert.IsTrue(t.IsFaulted);
                        Assert.IsInstanceOfType(t.Exception, typeof(AggregateException));
                        Assert.AreEqual(1, t.Exception.InnerExceptions.Count);
                        Assert.AreEqual(exception2, t.Exception.InnerException);
                    });
            }
        }

        [TestMethod]
        public void Finally_NoInputValue_CompletedTaskOfCancellation_RunsOnSameThreadAndDoesNotPostToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int originalThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = Int32.MinValue;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                TaskHelpers.Canceled()

                    // Act
                    .Finally(() =>
                        {
                            callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                        })

                    // Assert
                    .ContinueWith(task =>
                        {
                            Assert.AreEqual(originalThreadId, callbackThreadId);
                            syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Never());
                        });
            }
        }

        [TestMethod]
        public void Finally_NoInputValue_CompletedTaskOfFault_RunsOnSameThreadAndDoesNotPostToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int originalThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = Int32.MinValue;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                TaskHelpers.FromError(new InvalidOperationException())

                    // Act
                    .Finally(() =>
                        {
                            callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                        })

                    // Assert
                    .ContinueWith(task =>
                        {
                            var ex = task.Exception; // Observe the exception
                            Assert.AreEqual(originalThreadId, callbackThreadId);
                            syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Never());
                        });
            }
        }

        [TestMethod]
        public void Finally_NoInputValue_IncompleteTaskOfSuccess_RunsOnNewThreadAndPostsContinuationToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int originalThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = Int32.MinValue;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                Task incompleteTask = new Task(() => { });

                // Act
                Task resultTask = incompleteTask.Finally(() =>
                    {
                        callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                    });

                // Assert
                incompleteTask.Start();

                resultTask.ContinueWith(task =>
                    {
                        Assert.AreNotEqual(originalThreadId, callbackThreadId);
                        syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Once());
                    });
            }
        }

        [TestMethod]
        public void Finally_NoInputValue_IncompleteTaskOfCancellation_RunsOnNewThreadAndPostsContinuationToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int originalThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = Int32.MinValue;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                Task incompleteTask = new Task(() => { });
                Task resultTask = incompleteTask.ContinueWith(task => TaskHelpers.Canceled()).Unwrap();

                // Act
                resultTask = resultTask.Finally(() =>
                    {
                        callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                    });

                // Assert
                incompleteTask.Start();

                resultTask.ContinueWith(task =>
                    {
                        Assert.AreNotEqual(originalThreadId, callbackThreadId);
                        syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Once());
                    });
            }
        }

        [TestMethod]
        public void Finally_NoInputValue_IncompleteTaskOfFault_RunsOnNewThreadAndPostsContinuationToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int originalThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = Int32.MinValue;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                Task incompleteTask = new Task(() => { throw new InvalidOperationException(); });

                // Act
                Task resultTask = incompleteTask.Finally(() =>
                    {
                        callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                    });

                // Assert
                incompleteTask.Start();

                resultTask.ContinueWith(task =>
                    {
                        var ex = task.Exception; // Observe the exception
                        Assert.AreNotEqual(originalThreadId, callbackThreadId);
                        syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Once());
                    });
            }
        }

        // -----------------------------------------------------------------
        //  Task<T>.Finally(Action)

        [TestMethod]
        public void Finally_WithInputValue_CompletedTaskOfSuccess_RunsOnSameThreadAndDoesNotPostToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int originalThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = Int32.MinValue;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                TaskHelpers.FromResult(21)

                    // Act
                    .Finally(() =>
                        {
                            callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                        })

                    // Assert
                    .ContinueWith(task =>
                        {
                            Assert.AreEqual(21, task.Result);
                            Assert.AreEqual(originalThreadId, callbackThreadId);
                            syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Never());
                        });
            }
        }

        [TestMethod]
        public void Finally_WithInputValue_CompletedTaskOfFault_ExceptionInFinally()
        {
            using (new ThreadContextPreserver())
            {
                Exception exception1 = new InvalidOperationException("From source");
                Exception exception2 = new InvalidOperationException("FromFinally");

                // When the finally clause throws, that's the exception that propagates. 
                // Still ensure that the original exception from the try block is observed.

                // Act 
                Task<int> faultedTask = TaskHelpers.FromError<int>(exception1);
                Task<int> t = faultedTask.Finally(() => { throw exception2; });

                // Assert
                Assert.IsTrue(t.IsFaulted);
                Assert.IsInstanceOfType(t.Exception, typeof(AggregateException));
                Assert.AreEqual(1, t.Exception.InnerExceptions.Count);
                Assert.AreEqual(exception2, t.Exception.InnerException);
            }
        }

        [TestMethod]
        public void Finally_WithInputValue_IncompletedTask_ExceptionInFinally()
        {
            using (new ThreadContextPreserver())
            {
                Exception exception1 = new InvalidOperationException("From source");
                Exception exception2 = new InvalidOperationException("FromFinally");

                // Like test Finally_WithInputValue_CompletedTaskOfFault_ExceptionInFinally, but exercises when the original task doesn't complete synchronously

                // Act 
                Task<int> incompleteTask = new Task<int>(() => { throw exception1; });
                Task<int> t = incompleteTask.Finally(() => { throw exception2; });

                incompleteTask.Start();

                // Assert
                t.ContinueWith(prevTask =>
                    {
                        Assert.AreEqual(t, prevTask);

                        Assert.IsTrue(t.IsFaulted);
                        Assert.IsInstanceOfType(t.Exception, typeof(AggregateException));
                        Assert.AreEqual(1, t.Exception.InnerExceptions.Count);
                        Assert.AreEqual(exception2, t.Exception.InnerException);
                    });
            }
        }

        [TestMethod]
        public void Finally_WithInputValue_CompletedTaskOfCancellation_RunsOnSameThreadAndDoesNotPostToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int originalThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = Int32.MinValue;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                TaskHelpers.Canceled<int>()

                    // Act
                    .Finally(() =>
                        {
                            callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                        })

                    // Assert
                    .ContinueWith(task =>
                        {
                            Assert.AreEqual(originalThreadId, callbackThreadId);
                            syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Never());
                        });
            }
        }

        [TestMethod]
        public void Finally_WithInputValue_CompletedTaskOfFault_RunsOnSameThreadAndDoesNotPostToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int originalThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = Int32.MinValue;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                TaskHelpers.FromError<int>(new InvalidOperationException())

                    // Act
                    .Finally(() =>
                        {
                            callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                        })

                    // Assert
                    .ContinueWith(task =>
                        {
                            var ex = task.Exception; // Observe the exception
                            Assert.AreEqual(originalThreadId, callbackThreadId);
                            syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Never());
                        });
            }
        }

        [TestMethod]
        public void Finally_WithInputValue_IncompleteTaskOfSuccess_RunsOnNewThreadAndPostsContinuationToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int originalThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = Int32.MinValue;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                Task incompleteTask = new Task<int>(() => 21);

                // Act
                Task resultTask = incompleteTask.Finally(() =>
                    {
                        callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                    });

                // Assert
                incompleteTask.Start();

                resultTask.ContinueWith(task =>
                    {
                        Assert.AreNotEqual(originalThreadId, callbackThreadId);
                        syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Once());
                    });
            }
        }

        [TestMethod]
        public void Finally_WithInputValue_IncompleteTaskOfCancellation_RunsOnNewThreadAndPostsContinuationToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int originalThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = Int32.MinValue;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                Task<int> incompleteTask = new Task<int>(() => 42);
                Task resultTask = incompleteTask.ContinueWith(task => TaskHelpers.Canceled<int>()).Unwrap();

                // Act
                resultTask = resultTask.Finally(() =>
                    {
                        callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                    });

                // Assert
                incompleteTask.Start();

                resultTask.ContinueWith(task =>
                    {
                        Assert.AreNotEqual(originalThreadId, callbackThreadId);
                        syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Once());
                    });
            }
        }

        [TestMethod]
        public void Finally_WithInputValue_IncompleteTaskOfFault_RunsOnNewThreadAndPostsContinuationToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int originalThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = Int32.MinValue;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                Task<int> incompleteTask = new Task<int>(() => { throw new InvalidOperationException(); });

                // Act
                Task resultTask = incompleteTask.Finally(() =>
                    {
                        callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                    });

                // Assert
                incompleteTask.Start();

                resultTask.ContinueWith(task =>
                    {
                        var ex = task.Exception; // Observe the exception
                        Assert.AreNotEqual(originalThreadId, callbackThreadId);
                        syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Once());
                    });
            }
        }

        // -----------------------------------------------------------------
        //  Task Task.Then(Action)

        [TestMethod]
        public void Then_NoInputValue_NoReturnValue_CallsContinuation()
        {
            // Arrange
            bool ranContinuation = false;

            TaskHelpers.Completed()

            // Act
                              .Then(() =>
                              {
                                  ranContinuation = true;
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
                                  Assert.IsTrue(ranContinuation);
                              });
        }

        [TestMethod]
        public void Then_NoInputValue_NoReturnValue_ThrownExceptionIsRethrowd()
        {
            // Arrange
            TaskHelpers.Completed()

            // Act
                              .Then(() =>
                              {
                                  throw new NotImplementedException();
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.Faulted, task.Status);
                                  Assert.AreEqual(1, task.Exception.InnerExceptions.Count);
                                  var ex = task.Exception.InnerExceptions.Single();
                                  Assert.IsInstanceOfType(ex, typeof(NotImplementedException));
                              });
        }

        [TestMethod]
        public void Then_NoInputValue_NoReturnValue_FaultPreventsFurtherThenStatementsFromExecuting()
        {
            // Arrange
            bool ranContinuation = false;

            TaskHelpers.FromError(new NotImplementedException())

            // Act
                              .Then(() =>
                              {
                                  ranContinuation = true;
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  var ex = task.Exception;  // Observe the exception
                                  Assert.IsFalse(ranContinuation);
                              });
        }

        [TestMethod]
        public void Then_NoInputValue_NoReturnValue_ManualCancellationPreventsFurtherThenStatementsFromExecuting()
        {
            // Arrange
            bool ranContinuation = false;

            TaskHelpers.Canceled()

            // Act
                              .Then(() =>
                              {
                                  ranContinuation = true;
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.Canceled, task.Status);
                                  Assert.IsFalse(ranContinuation);
                              });
        }

        [TestMethod]
        public void Then_NoInputValue_NoReturnValue_TokenCancellationPreventsFurtherThenStatementsFromExecuting()
        {
            // Arrange
            bool ranContinuation = false;
            CancellationToken cancellationToken = new CancellationToken(canceled: true);

            TaskHelpers.Completed()

            // Act
                              .Then(() =>
                              {
                                  ranContinuation = true;
                              }, cancellationToken)

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.Canceled, task.Status);
                                  Assert.IsFalse(ranContinuation);
                              });
        }

        [TestMethod]
        public void Then_NoInputValue_NoReturnValue_IncompleteTask_RunsOnNewThreadAndPostsContinuationToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int originalThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = Int32.MinValue;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                Task incompleteTask = new Task(() => { });

                // Act
                Task resultTask = incompleteTask.Then(() =>
                    {
                        callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                    });

                // Assert
                incompleteTask.Start();

                resultTask.ContinueWith(task =>
                    {
                        Assert.AreNotEqual(originalThreadId, callbackThreadId);
                        syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Once());
                    });
            }
        }

        [TestMethod]
        public void Then_NoInputValue_NoReturnValue_CompleteTask_RunsOnSameThreadAndDoesNotPostToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int originalThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = Int32.MinValue;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                TaskHelpers.Completed()

                    // Act
                    .Then(() =>
                        {
                            callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                        })

                    // Assert
                    .ContinueWith(task =>
                        {
                            Assert.AreEqual(originalThreadId, callbackThreadId);
                            syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Never());
                        });
            }
        }

        // -----------------------------------------------------------------
        //  Task Task.Then(Func<Task>)

        [TestMethod]
        public void Then_NoInputValue_ReturnsTask_CallsContinuation()
        {
            // Arrange
            bool ranContinuation = false;

            TaskHelpers.Completed()

            // Act
                              .Then(() =>
                              {
                                  ranContinuation = true;
                                  return TaskHelpers.Completed();
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
                                  Assert.IsTrue(ranContinuation);
                              });
        }

        [TestMethod]
        public void Then_NoInputValue_ReturnsTask_ThrownExceptionIsRethrowd()
        {
            // Arrange
            TaskHelpers.Completed()

            // Act
                              .Then(() =>
                              {
                                  throw new NotImplementedException();
                                  return TaskHelpers.Completed();  // Return-after-throw to guarantee correct lambda signature
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.Faulted, task.Status);
                                  Assert.AreEqual(1, task.Exception.InnerExceptions.Count);
                                  var ex = task.Exception.InnerExceptions.Single();
                                  Assert.IsInstanceOfType(ex, typeof(NotImplementedException));
                              });
        }

        [TestMethod]
        public void Then_NoInputValue_ReturnsTask_FaultPreventsFurtherThenStatementsFromExecuting()
        {
            // Arrange
            bool ranContinuation = false;

            TaskHelpers.FromError(new NotImplementedException())

            // Act
                              .Then(() =>
                              {
                                  ranContinuation = true;
                                  return TaskHelpers.Completed();
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  var ex = task.Exception;  // Observe the exception
                                  Assert.IsFalse(ranContinuation);
                              });
        }

        [TestMethod]
        public void Then_NoInputValue_ReturnsTask_ManualCancellationPreventsFurtherThenStatementsFromExecuting()
        {
            // Arrange
            bool ranContinuation = false;

            TaskHelpers.Canceled()

            // Act
                              .Then(() =>
                              {
                                  ranContinuation = true;
                                  return TaskHelpers.Completed();
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.Canceled, task.Status);
                                  Assert.IsFalse(ranContinuation);
                              });
        }

        [TestMethod]
        public void Then_NoInputValue_ReturnsTask_TokenCancellationPreventsFurtherThenStatementsFromExecuting()
        {
            // Arrange
            bool ranContinuation = false;
            CancellationToken cancellationToken = new CancellationToken(canceled: true);

            TaskHelpers.Completed()

            // Act
                              .Then(() =>
                              {
                                  ranContinuation = true;
                                  return TaskHelpers.Completed();
                              }, cancellationToken)

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.Canceled, task.Status);
                                  Assert.IsFalse(ranContinuation);
                              });
        }

        [TestMethod]
        public void Then_NoInputValue_ReturnsTask_IncompleteTask_RunsOnNewThreadAndPostsContinuationToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int originalThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = Int32.MinValue;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                Task incompleteTask = new Task(() => { });

                // Act
                Task resultTask = incompleteTask.Then(() =>
                    {
                        callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                        return TaskHelpers.Completed();
                    });

                // Assert
                incompleteTask.Start();

                resultTask.ContinueWith(task =>
                    {
                        Assert.AreNotEqual(originalThreadId, callbackThreadId);
                        syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Once());
                    });
            }
        }

        [TestMethod]
        public void Then_NoInputValue_ReturnsTask_CompleteTask_RunsOnSameThreadAndDoesNotPostToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int originalThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = Int32.MinValue;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                TaskHelpers.Completed()

                    // Act
                    .Then(() =>
                        {
                            callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                            return TaskHelpers.Completed();
                        })

                    // Assert
                    .ContinueWith(task =>
                        {
                            Assert.AreEqual(originalThreadId, callbackThreadId);
                            syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Never());
                        });
            }
        }

        // -----------------------------------------------------------------
        //  Task<TOut> Task.Then(Func<TOut>)

        [TestMethod]
        public void Then_NoInputValue_WithReturnValue_CallsContinuation()
        {
            // Arrange
            TaskHelpers.Completed()

            // Act
                              .Then(() =>
                              {
                                  return 42;
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
                                  Assert.AreEqual(42, task.Result);
                              });
        }

        [TestMethod]
        public void Then_NoInputValue_WithReturnValue_ThrownExceptionIsRethrowd()
        {
            // Arrange
            TaskHelpers.Completed()

            // Act
                              .Then(() =>
                              {
                                  throw new NotImplementedException();
                                  return 0;  // Return-after-throw to guarantee correct lambda signature
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.Faulted, task.Status);
                                  Assert.AreEqual(1, task.Exception.InnerExceptions.Count);
                                  var ex = task.Exception.InnerExceptions.Single();
                                  Assert.IsInstanceOfType(ex, typeof(NotImplementedException));
                              });
        }

        [TestMethod]
        public void Then_NoInputValue_WithReturnValue_FaultPreventsFurtherThenStatementsFromExecuting()
        {
            // Arrange
            bool ranContinuation = false;

            TaskHelpers.FromError(new NotImplementedException())

            // Act
                              .Then(() =>
                              {
                                  ranContinuation = true;
                                  return 42;
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  var ex = task.Exception;  // Observe the exception
                                  Assert.IsFalse(ranContinuation);
                              });
        }

        [TestMethod]
        public void Then_NoInputValue_WithReturnValue_ManualCancellationPreventsFurtherThenStatementsFromExecuting()
        {
            // Arrange
            bool ranContinuation = false;

            TaskHelpers.Canceled()

            // Act
                              .Then(() =>
                              {
                                  ranContinuation = true;
                                  return 42;
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.Canceled, task.Status);
                                  Assert.IsFalse(ranContinuation);
                              });
        }

        [TestMethod]
        public void Then_NoInputValue_WithReturnValue_TokenCancellationPreventsFurtherThenStatementsFromExecuting()
        {
            // Arrange
            bool ranContinuation = false;
            CancellationToken cancellationToken = new CancellationToken(canceled: true);

            TaskHelpers.Completed()

            // Act
                              .Then(() =>
                              {
                                  ranContinuation = true;
                                  return 42;
                              }, cancellationToken)

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.Canceled, task.Status);
                                  Assert.IsFalse(ranContinuation);
                              });
        }

        [TestMethod]
        public void Then_NoInputValue_WithReturnValue_IncompleteTask_RunsOnNewThreadAndPostsContinuationToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int originalThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = Int32.MinValue;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                Task incompleteTask = new Task(() => { });

                // Act
                Task resultTask = incompleteTask.Then(() =>
                    {
                        callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                        return 42;
                    });

                // Assert
                incompleteTask.Start();

                resultTask.ContinueWith(task =>
                    {
                        Assert.AreNotEqual(originalThreadId, callbackThreadId);
                        syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Once());
                    });
            }
        }

        [TestMethod]
        public void Then_NoInputValue_WithReturnValue_CompleteTask_RunsOnSameThreadAndDoesNotPostToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int originalThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = Int32.MinValue;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                TaskHelpers.Completed()

                    // Act
                    .Then(() =>
                        {
                            callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                            return 42;
                        })

                    // Assert
                    .ContinueWith(task =>
                        {
                            Assert.AreEqual(originalThreadId, callbackThreadId);
                            syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Never());
                        });
            }
        }

        // -----------------------------------------------------------------
        //  Task<TOut> Task.Then(Func<Task<TOut>>)

        [TestMethod]
        public void Then_NoInputValue_WithTaskReturnValue_CallsContinuation()
        {
            // Arrange
            TaskHelpers.Completed()

            // Act
                              .Then(() =>
                              {
                                  return TaskHelpers.FromResult(42);
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
                                  Assert.AreEqual(42, task.Result);
                              });
        }

        [TestMethod]
        public void Then_NoInputValue_WithTaskReturnValue_ThrownExceptionIsRethrowd()
        {
            // Arrange
            TaskHelpers.Completed()

            // Act
                              .Then(() =>
                              {
                                  throw new NotImplementedException();
                                  return TaskHelpers.FromResult(0);  // Return-after-throw to guarantee correct lambda signature
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.Faulted, task.Status);
                                  Assert.AreEqual(1, task.Exception.InnerExceptions.Count);
                                  var ex = task.Exception.InnerExceptions.Single();
                                  Assert.IsInstanceOfType(ex, typeof(NotImplementedException));

                              });
        }

        [TestMethod]
        public void Then_NoInputValue_WithTaskReturnValue_FaultPreventsFurtherThenStatementsFromExecuting()
        {
            // Arrange
            bool ranContinuation = false;

            TaskHelpers.FromError(new NotImplementedException())

            // Act
                              .Then(() =>
                              {
                                  ranContinuation = true;
                                  return TaskHelpers.FromResult(42);
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  var ex = task.Exception;  // Observe the exception
                                  Assert.IsFalse(ranContinuation);
                              });
        }

        [TestMethod]
        public void Then_NoInputValue_WithTaskReturnValue_ManualCancellationPreventsFurtherThenStatementsFromExecuting()
        {
            // Arrange
            bool ranContinuation = false;

            TaskHelpers.Canceled()

            // Act
                              .Then(() =>
                              {
                                  ranContinuation = true;
                                  return TaskHelpers.FromResult(42);
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.Canceled, task.Status);
                                  Assert.IsFalse(ranContinuation);
                              });
        }

        [TestMethod]
        public void Then_NoInputValue_WithTaskReturnValue_TokenCancellationPreventsFurtherThenStatementsFromExecuting()
        {
            // Arrange
            bool ranContinuation = false;
            CancellationToken cancellationToken = new CancellationToken(canceled: true);

            TaskHelpers.Completed()

            // Act
                              .Then(() =>
                              {
                                  ranContinuation = true;
                                  return TaskHelpers.FromResult(42);
                              }, cancellationToken)

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.Canceled, task.Status);
                                  Assert.IsFalse(ranContinuation);
                              });
        }

        [TestMethod]
        public void Then_NoInputValue_WithTaskReturnValue_IncompleteTask_RunsOnNewThreadAndPostsContinuationToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int originalThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = Int32.MinValue;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                Task incompleteTask = new Task(() => { });

                // Act
                Task resultTask = incompleteTask.Then(() =>
                    {
                        callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                        return TaskHelpers.FromResult(42);
                    });

                // Assert
                incompleteTask.Start();

                resultTask.ContinueWith(task =>
                    {
                        Assert.AreNotEqual(originalThreadId, callbackThreadId);
                        syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Once());
                    });
            }
        }

        [TestMethod]
        public void Then_NoInputValue_WithTaskReturnValue_CompleteTask_RunsOnSameThreadAndDoesNotPostToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int originalThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = Int32.MinValue;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                TaskHelpers.Completed()

                    // Act
                    .Then(() =>
                        {
                            callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                            return TaskHelpers.FromResult(42);
                        })

                    // Assert
                    .ContinueWith(task =>
                        {
                            Assert.AreEqual(originalThreadId, callbackThreadId);
                            syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Never());
                        });
            }
        }

        // -----------------------------------------------------------------
        //  Task Task<TIn>.Then(Action<TIn>)

        [TestMethod]
        public void Then_WithInputValue_NoReturnValue_CallsContinuationWithPriorTaskResult()
        {
            // Arrange
            int passedResult = 0;

            TaskHelpers.FromResult(21)

            // Act
                              .Then(result =>
                              {
                                  passedResult = result;
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
                                  Assert.AreEqual(21, passedResult);
                              });
        }

        [TestMethod]
        public void Then_WithInputValue_NoReturnValue_ThrownExceptionIsRethrowd()
        {
            // Arrange
            TaskHelpers.FromResult(21)

            // Act
                              .Then(result =>
                              {
                                  throw new NotImplementedException();
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.Faulted, task.Status);
                                  Assert.AreEqual(1, task.Exception.InnerExceptions.Count);
                                  var ex = task.Exception.InnerExceptions.Single();
                                  Assert.IsInstanceOfType(ex, typeof(NotImplementedException));
                              });
        }

        [TestMethod]
        public void Then_WithInputValue_NoReturnValue_FaultPreventsFurtherThenStatementsFromExecuting()
        {
            // Arrange
            bool ranContinuation = false;

            TaskHelpers.FromError<int>(new NotImplementedException())

            // Act
                              .Then(result =>
                              {
                                  ranContinuation = true;
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  var ex = task.Exception;  // Observe the exception
                                  Assert.IsFalse(ranContinuation);
                              });
        }

        [TestMethod]
        public void Then_WithInputValue_NoReturnValue_ManualCancellationPreventsFurtherThenStatementsFromExecuting()
        {
            // Arrange
            bool ranContinuation = false;

            TaskHelpers.Canceled<int>()

            // Act
                              .Then(result =>
                              {
                                  ranContinuation = true;
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.Canceled, task.Status);
                                  Assert.IsFalse(ranContinuation);
                              });
        }

        [TestMethod]
        public void Then_WithInputValue_NoReturnValue_TokenCancellationPreventsFurtherThenStatementsFromExecuting()
        {
            // Arrange
            bool ranContinuation = false;
            CancellationToken cancellationToken = new CancellationToken(canceled: true);

            TaskHelpers.FromResult(21)

            // Act
                              .Then(result =>
                              {
                                  ranContinuation = true;
                              }, cancellationToken)

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.Canceled, task.Status);
                                  Assert.IsFalse(ranContinuation);
                              });
        }

        [TestMethod]
        public void Then_WithInputValue_NoReturnValue_IncompleteTask_RunsOnNewThreadAndPostsContinuationToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int originalThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = Int32.MinValue;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                Task<int> incompleteTask = new Task<int>(() => 21);

                // Act
                Task resultTask = incompleteTask.Then(result =>
                    {
                        callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                    });

                // Assert
                incompleteTask.Start();

                resultTask.ContinueWith(task =>
                    {
                        Assert.AreNotEqual(originalThreadId, callbackThreadId);
                        syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Once());
                    });
            }
        }

        [TestMethod]
        public void Then_WithInputValue_NoReturnValue_CompleteTask_RunsOnSameThreadAndDoesNotPostToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int originalThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = Int32.MinValue;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                TaskHelpers.FromResult(21)

                    // Act
                    .Then(result =>
                        {
                            callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                        })

                    // Assert
                    .ContinueWith(task =>
                        {
                            Assert.AreEqual(originalThreadId, callbackThreadId);
                            syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Never());
                        });
            }
        }

        // -----------------------------------------------------------------
        //  Task<TOut> Task<TIn>.Then(Func<TIn, TOut>)

        [TestMethod]
        public void Then_WithInputValue_WithReturnValue_CallsContinuation()
        {
            // Arrange
            TaskHelpers.FromResult(21)

            // Act
                              .Then(result =>
                              {
                                  return 42;
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
                                  Assert.AreEqual(42, task.Result);
                              });
        }

        [TestMethod]
        public void Then_WithInputValue_WithReturnValue_ThrownExceptionIsRethrowd()
        {
            // Arrange
            TaskHelpers.FromResult(21)

            // Act
                              .Then(result =>
                              {
                                  throw new NotImplementedException();
                                  return 0;  // Return-after-throw to guarantee correct lambda signature
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.Faulted, task.Status); 
                                  Assert.AreEqual(1, task.Exception.InnerExceptions.Count);
                                  var ex = task.Exception.InnerExceptions.Single();
                                  Assert.IsInstanceOfType(ex, typeof(NotImplementedException));

                              });
        }

        [TestMethod]
        public void Then_WithInputValue_WithReturnValue_FaultPreventsFurtherThenStatementsFromExecuting()
        {
            // Arrange
            bool ranContinuation = false;

            TaskHelpers.FromError<int>(new NotImplementedException())

            // Act
                              .Then(result =>
                              {
                                  ranContinuation = true;
                                  return 42;
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  var ex = task.Exception;  // Observe the exception
                                  Assert.IsFalse(ranContinuation);
                              });
        }

        [TestMethod]
        public void Then_WithInputValue_WithReturnValue_ManualCancellationPreventsFurtherThenStatementsFromExecuting()
        {
            // Arrange
            bool ranContinuation = false;

            TaskHelpers.Canceled<int>()

            // Act
                              .Then(result =>
                              {
                                  ranContinuation = true;
                                  return 42;
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.Canceled, task.Status);
                                  Assert.IsFalse(ranContinuation);
                              });
        }

        [TestMethod]
        public void Then_WithInputValue_WithReturnValue_TokenCancellationPreventsFurtherThenStatementsFromExecuting()
        {
            // Arrange
            bool ranContinuation = false;
            CancellationToken cancellationToken = new CancellationToken(canceled: true);

            TaskHelpers.FromResult(21)

            // Act
                              .Then(result =>
                              {
                                  ranContinuation = true;
                                  return 42;
                              }, cancellationToken)

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.Canceled, task.Status);
                                  Assert.IsFalse(ranContinuation);
                              });
        }

        [TestMethod]
        public void Then_WithInputValue_WithReturnValue_IncompleteTask_RunsOnNewThreadAndPostsContinuationToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int originalThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = Int32.MinValue;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                Task<int> incompleteTask = new Task<int>(() => 21);

                // Act
                Task resultTask = incompleteTask.Then(result =>
                    {
                        callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                        return 42;
                    });

                // Assert
                incompleteTask.Start();

                resultTask.ContinueWith(task =>
                    {
                        Assert.AreNotEqual(originalThreadId, callbackThreadId);
                        syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Once());
                    });
            }
        }

        [TestMethod]
        public void Then_WithInputValue_WithReturnValue_CompleteTask_RunsOnSameThreadAndDoesNotPostToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int originalThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = Int32.MinValue;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                TaskHelpers.FromResult(21)

                    // Act
                    .Then(result =>
                        {
                            callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                            return 42;
                        })

                    // Assert
                    .ContinueWith(task =>
                        {
                            Assert.AreEqual(originalThreadId, callbackThreadId);
                            syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Never());
                        });
            }
        }

        // -----------------------------------------------------------------
        //  Task Task<TIn>.Then(Func<TIn, Task>)

        [TestMethod]
        public void Then_WithInputValue_ReturnsTask_CallsContinuation()
        {
            // Arrange
            TaskHelpers.FromResult(21)

            // Act
                              .Then(result =>
                              {
                                  return TaskHelpers.Completed();
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
                              });
        }

        [TestMethod]
        public void Then_WithInputValue_ReturnsTask_ThrownExceptionIsRethrowd()
        {
            // Arrange
            TaskHelpers.FromResult(21)

            // Act
                              .Then(result =>
                              {
                                  throw new NotImplementedException();
                                  return TaskHelpers.Completed();  // Return-after-throw to guarantee correct lambda signature
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.Faulted, task.Status);
                                  Assert.AreEqual(1, task.Exception.InnerExceptions.Count);
                                  var ex = task.Exception.InnerExceptions.Single();
                                  Assert.IsInstanceOfType(ex, typeof(NotImplementedException));
                              });
        }

        [TestMethod]
        public void Then_WithInputValue_ReturnsTask_FaultPreventsFurtherThenStatementsFromExecuting()
        {
            // Arrange
            bool ranContinuation = false;

            TaskHelpers.FromError<int>(new NotImplementedException())

            // Act
                              .Then(result =>
                              {
                                  ranContinuation = true;
                                  return TaskHelpers.Completed();
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  var ex = task.Exception;  // Observe the exception
                                  Assert.IsFalse(ranContinuation);
                              });
        }

        [TestMethod]
        public void Then_WithInputValue_ReturnsTask_ManualCancellationPreventsFurtherThenStatementsFromExecuting()
        {
            // Arrange
            bool ranContinuation = false;

            TaskHelpers.Canceled<int>()

            // Act
                              .Then(result =>
                              {
                                  ranContinuation = true;
                                  return TaskHelpers.Completed();
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.Canceled, task.Status);
                                  Assert.IsFalse(ranContinuation);
                              });
        }

        [TestMethod]
        public void Then_WithInputValue_ReturnsTask_TokenCancellationPreventsFurtherThenStatementsFromExecuting()
        {
            // Arrange
            bool ranContinuation = false;
            CancellationToken cancellationToken = new CancellationToken(canceled: true);

            TaskHelpers.FromResult(21)

            // Act
                              .Then(result =>
                              {
                                  ranContinuation = true;
                                  return TaskHelpers.Completed();
                              }, cancellationToken)

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.Canceled, task.Status);
                                  Assert.IsFalse(ranContinuation);
                              });
        }

        [TestMethod]
        public void Then_WithInputValue_ReturnsTask_IncompleteTask_RunsOnNewThreadAndPostsContinuationToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int originalThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = Int32.MinValue;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                Task<int> incompleteTask = new Task<int>(() => 21);

                // Act
                Task resultTask = incompleteTask.Then(result =>
                    {
                        callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                        return TaskHelpers.Completed();
                    });

                // Assert
                incompleteTask.Start();

                resultTask.ContinueWith(task =>
                    {
                        Assert.AreNotEqual(originalThreadId, callbackThreadId);
                        syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Once());
                    });
            }
        }

        [TestMethod]
        public void Then_WithInputValue_ReturnsTask_CompleteTask_RunsOnSameThreadAndDoesNotPostToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int originalThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = Int32.MinValue;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                TaskHelpers.FromResult(21)

                    // Act
                    .Then(result =>
                        {
                            callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                            return TaskHelpers.Completed();
                        })

                    // Assert
                    .ContinueWith(task =>
                        {
                            Assert.AreEqual(originalThreadId, callbackThreadId);
                            syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Never());
                        });
            }
        }

        // -----------------------------------------------------------------
        //  Task<TOut> Task<TIn>.Then(Func<TIn, Task<TOut>>)

        [TestMethod]
        public void Then_WithInputValue_WithTaskReturnValue_CallsContinuation()
        {
            // Arrange
            TaskHelpers.FromResult(21)

            // Act
                              .Then(result =>
                              {
                                  return TaskHelpers.FromResult(42);
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
                                  Assert.AreEqual(42, task.Result);
                              });
        }

        [TestMethod]
        public void Then_WithInputValue_WithTaskReturnValue_ThrownExceptionIsRethrowd()
        {
            // Arrange
            TaskHelpers.FromResult(21)

            // Act
                              .Then(result =>
                              {
                                  throw new NotImplementedException();
                                  return TaskHelpers.FromResult(0);  // Return-after-throw to guarantee correct lambda signature
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.Faulted, task.Status);
                                  Assert.AreEqual(1, task.Exception.InnerExceptions.Count);
                                  var ex = task.Exception.InnerExceptions.Single();
                                  Assert.IsInstanceOfType(ex, typeof(NotImplementedException));
                              });
        }

        [TestMethod]
        public void Then_WithInputValue_WithTaskReturnValue_FaultPreventsFurtherThenStatementsFromExecuting()
        {
            // Arrange
            bool ranContinuation = false;

            TaskHelpers.FromError<int>(new NotImplementedException())

            // Act
                              .Then(result =>
                              {
                                  ranContinuation = true;
                                  return TaskHelpers.FromResult(42);
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  var ex = task.Exception;  // Observe the exception
                                  Assert.IsFalse(ranContinuation);
                              });
        }

        [TestMethod]
        public void Then_WithInputValue_WithTaskReturnValue_ManualCancellationPreventsFurtherThenStatementsFromExecuting()
        {
            // Arrange
            bool ranContinuation = false;

            TaskHelpers.Canceled<int>()

            // Act
                              .Then(result =>
                              {
                                  ranContinuation = true;
                                  return TaskHelpers.FromResult(42);
                              })

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.Canceled, task.Status);
                                  Assert.IsFalse(ranContinuation);
                              });
        }

        [TestMethod]
        public void Then_WithInputValue_WithTaskReturnValue_TokenCancellationPreventsFurtherThenStatementsFromExecuting()
        {
            // Arrange
            bool ranContinuation = false;
            CancellationToken cancellationToken = new CancellationToken(canceled: true);

            TaskHelpers.FromResult(21)

            // Act
                              .Then(result =>
                              {
                                  ranContinuation = true;
                                  return TaskHelpers.FromResult(42);
                              }, cancellationToken)

            // Assert
                              .ContinueWith(task =>
                              {
                                  Assert.AreEqual(TaskStatus.Canceled, task.Status);
                                  Assert.IsFalse(ranContinuation);
                              });
        }

        [TestMethod]
        public void Then_WithInputValue_WithTaskReturnValue_IncompleteTask_RunsOnNewThreadAndPostsContinuationToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int originalThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = Int32.MinValue;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                Task<int> incompleteTask = new Task<int>(() => 21);

                // Act
                Task resultTask = incompleteTask.Then(result =>
                    {
                        callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                        return TaskHelpers.FromResult(42);
                    });

                // Assert
                incompleteTask.Start();

                resultTask.ContinueWith(task =>
                    {
                        Assert.AreNotEqual(originalThreadId, callbackThreadId);
                        syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Once());
                    });
            }
        }

        [TestMethod]
        public void Then_WithInputValue_WithTaskReturnValue_CompleteTask_RunsOnSameThreadAndDoesNotPostToSynchronizationContext()
        {
            using (new ThreadContextPreserver())
            {
                // Arrange
                int originalThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = Int32.MinValue;
                var syncContext = new Mock<SynchronizationContext> { CallBase = true };
                SynchronizationContext.SetSynchronizationContext(syncContext.Object);

                TaskHelpers.FromResult(21)

                    // Act
                    .Then(result =>
                        {
                            callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                            return TaskHelpers.FromResult(42);
                        })

                    // Assert
                    .ContinueWith(task =>
                        {
                            Assert.AreEqual(originalThreadId, callbackThreadId);
                            syncContext.Verify(sc => sc.Post(It.IsAny<SendOrPostCallback>(), null), Times.Never());
                        });
            }
        }

        // -----------------------------------------------------------------
        //  bool Task.TryGetResult(Task<TResult>, out TResult)

        [TestMethod]
        public void TryGetResult_CompleteTask_ReturnsTrueAndGivesResult()
        {
            // Arrange
            var task = TaskHelpers.FromResult(42);

            // Act
            int value;
            bool result = task.TryGetResult(out value);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(42, value);
        }

        [TestMethod]
        public void TryGetResult_FaultedTask_ReturnsFalse()
        {
            // Arrange
            var task = TaskHelpers.FromError<int>(new Exception());

            // Act
            int value;
            bool result = task.TryGetResult(out value);

            // Assert
            Assert.IsFalse(result);
            var ex = task.Exception; // Observe the task exception
        }

        [TestMethod]
        public void TryGetResult_CanceledTask_ReturnsFalse()
        {
            // Arrange
            var task = TaskHelpers.Canceled<int>();

            // Act
            int value;
            bool result = task.TryGetResult(out value);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryGetResult_IncompleteTask_ReturnsFalse()
        {
            // Arrange
            var incompleteTask = new Task<int>(() => 42);

            // Act
            int value;
            bool result = incompleteTask.TryGetResult(out value);

            // Assert
            Assert.IsFalse(result);

            incompleteTask.Start();
            var status = incompleteTask.Status;  // Make sure the task gets observed
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