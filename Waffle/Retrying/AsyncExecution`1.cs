namespace Waffle.Retrying
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Internal;
    using Waffle.Properties;
    using Waffle.Tasks;

    internal class AsyncExecution<TResult>
    {
        private readonly Func<Task<TResult>> taskFunc;
        private readonly ShouldRetry shouldRetry;
        private readonly Func<Exception, bool> isTransient;
        private readonly Action<int, Exception, TimeSpan> onRetrying;
        private readonly bool fastFirstRetry;
        private readonly CancellationToken cancellationToken;
        private Task<TResult> previousTask;
        private int retryCount;

        public AsyncExecution(Func<Task<TResult>> taskFunc, ShouldRetry shouldRetry, Func<Exception, bool> isTransient, Action<int, Exception, TimeSpan> onRetrying, bool fastFirstRetry, CancellationToken cancellationToken)
        {
            Contract.Requires(taskFunc != null);

            this.taskFunc = taskFunc;
            this.shouldRetry = shouldRetry;
            this.isTransient = isTransient;
            this.onRetrying = onRetrying;
            this.fastFirstRetry = fastFirstRetry;
            this.cancellationToken = cancellationToken;
        }

        internal Task<TResult> ExecuteAsync()
        {
            return this.ExecuteAsyncImpl();
        }

        private Task<TResult> ExecuteAsyncImpl()
        {
            if (this.cancellationToken.IsCancellationRequested)
            {
                if (this.previousTask != null)
                {
                    return this.previousTask;
                }

                return TaskHelpers.Canceled<TResult>();
            }

            Task<TResult> task;
            try
            {
                task = this.taskFunc();
            }
            catch (Exception ex)
            {
                if (!this.isTransient(ex))
                {
                    throw;
                }

                task = TaskHelpers.FromError<TResult>(ex);
            }

            if (task == null)
            {
                throw Error.Argument("taskFunc", Resources.TaskCannotBeNull);
            }

            if (task.Status == TaskStatus.RanToCompletion)
            {
                return task;
            }

            if (task.Status == TaskStatus.Created)
            {
                throw Error.Argument("taskFunc", Resources.TaskMustBeScheduled);
            }
                        
            return task.ContinueWith<Task<TResult>>(this.ExecuteAsyncContinueWith, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default).Unwrap<TResult>();
        }

        private Task<TResult> ExecuteAsyncContinueWith(Task<TResult> runningTask)
        {
            Contract.Requires(runningTask != null);

            if (!runningTask.IsFaulted || this.cancellationToken.IsCancellationRequested)
            {
                return runningTask;
            }

            TimeSpan time;
            Exception innerException = runningTask.Exception.InnerException;

            if (!this.isTransient(innerException) || !this.shouldRetry(this.retryCount++, innerException, out time))
            {
                return runningTask;
            }

            if (time < TimeSpan.Zero)
            {
                time = TimeSpan.Zero;
            }

            this.onRetrying(this.retryCount, innerException, time);
            this.previousTask = runningTask;
            if (time > TimeSpan.Zero && (this.retryCount > 1 || !this.fastFirstRetry))
            {
                return this.WaitAndExecuteAsync(time);
            }

            return this.ExecuteAsyncImpl();
        }

        private async Task<TResult> WaitAndExecuteAsync(TimeSpan time)
        {
            await Task.Delay(time, this.cancellationToken);
            return await this.ExecuteAsyncImpl();
        }
    }
}