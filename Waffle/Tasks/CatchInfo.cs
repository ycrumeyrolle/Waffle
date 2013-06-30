namespace Waffle.Tasks
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    internal class CatchInfo : CatchInfoBase<Task>
    {
        private static readonly CatchResult Completed = new CatchResult { Task = TaskHelpers.Completed() };

        public CatchInfo(Task task)
            : base(task)
        {
        }

        /// <summary>
        /// Returns a CatchResult that returns a completed (non-faulted) task.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This would result in poor usability.")]
        public CatchResult Handled()
        {
            return Completed;
        }

        /// <summary>
        /// Returns a CatchResult that executes the given task and returns it, in whatever state it finishes.
        /// </summary>
        /// <param name="task">The task to return.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This would result in poor usability.")]
        public CatchResult Task(Task task)
        {
            return new CatchResult { Task = task };
        }

        /// <summary>
        /// Returns a CatchResult that throws the given exception.
        /// </summary>
        /// <param name="ex">The exception to throw.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This would result in poor usability.")]
        public CatchResult Throw(Exception ex)
        {
            return new CatchResult { Task = TaskHelpers.FromError<object>(ex) };
        }
    }
}