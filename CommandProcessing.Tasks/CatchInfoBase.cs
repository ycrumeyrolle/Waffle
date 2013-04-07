namespace CommandProcessing.Tasks
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;

    internal abstract class CatchInfoBase<TTask>
        where TTask : Task
    {
        private readonly Exception exception;
        private readonly TTask task;

        protected CatchInfoBase(TTask task)
        {
            Contract.Assert(task != null);
            this.task = task;
            this.exception = this.task.Exception.GetBaseException();  // Observe the exception early, to prevent tasks tearing down the app domain
        }

        /// <summary>
        /// The exception that was thrown to cause the Catch block to execute.
        /// </summary>
        public Exception Exception
        {
            get { return this.exception; }
        }

        /// <summary>
        /// Returns a CatchResult that re-throws the original exception.
        /// </summary>
        public CatchResult Throw()
        {
            return new CatchResult { Task = this.task };
        }

        /// <summary>
        /// Represents a result to be returned from a Catch handler.
        /// </summary>
        internal struct CatchResult
        {
            /// <summary>
            /// Gets or sets the task to be returned to the caller.
            /// </summary>
            internal TTask Task { get; set; }
        }
    }
}