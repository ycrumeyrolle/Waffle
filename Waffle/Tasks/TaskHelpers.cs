namespace Waffle.Tasks
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    /// <summary>
    /// Helpers for safely using Task libraries. 
    /// </summary>
    internal static class TaskHelpers
    {
        private static readonly Task DefaultCompleted = Task.FromResult(default(VoidTaskResult));

        private static readonly Task<object> CompletedTaskReturningNull = Task.FromResult<object>(null);

        /// <summary>
        /// Returns a canceled Task. The task is completed, IsCanceled = True, IsFaulted = False.
        /// </summary>
        internal static Task Canceled()
        {
            return CancelCache<VoidTaskResult>.CanceledTask;
        }

        /// <summary>
        /// Returns a canceled Task of the given type. The task is completed, IsCanceled = True, IsFaulted = False.
        /// </summary>
        internal static Task<TResult> Canceled<TResult>()
        {
            return CancelCache<TResult>.CanceledTask;
        }

        /// <summary>
        /// Returns a completed task that has no result. 
        /// </summary>        
        internal static Task Completed()
        {
            return DefaultCompleted;
        }

        /// <summary>
        /// Returns a completed task that has no result. 
        /// </summary>        
        internal static Task<TResult> Completed<TResult>()
        {
            return Task.FromResult(default(TResult));
        }

        /// <summary>
        /// Returns an error task. The task is Completed, IsCanceled = False, IsFaulted = True.
        /// </summary>
        internal static Task FromError(Exception exception)
        {
            return FromError<VoidTaskResult>(exception);
        }

        /// <summary>
        /// Returns an error task of the given type. The task is Completed, IsCanceled = False, IsFaulted = True.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        internal static Task<TResult> FromError<TResult>(Exception exception)
        {
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            tcs.SetException(exception);
            return tcs.Task;
        }
        
        internal static Task<object> NullResult()
        {
            return CompletedTaskReturningNull;
        }
    
        /// <summary>
        /// Used as the T in a "conversion" of a Task into a Task{T}.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private struct VoidTaskResult
        {
        }

        /// <summary>
        /// This class is a convenient cache for per-type cancelled tasks.
        /// </summary>
        private static class CancelCache<TResult>
        {
            public static readonly Task<TResult> CanceledTask = GetCancelledTask();

            private static Task<TResult> GetCancelledTask()
            {
                TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
                tcs.SetCanceled();
                return tcs.Task;
            }
        }
    }
}
