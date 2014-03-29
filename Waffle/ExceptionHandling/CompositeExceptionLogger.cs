namespace Waffle.ExceptionHandling
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Internal;
    using Waffle.Properties;

    internal class CompositeExceptionLogger : IExceptionLogger
    {
        private readonly IExceptionLogger[] loggers;

        public CompositeExceptionLogger(params IExceptionLogger[] loggers)
            : this((IEnumerable<IExceptionLogger>)loggers)
        {
        }

        public CompositeExceptionLogger(IEnumerable<IExceptionLogger> loggers)
        {
            if (loggers == null)
            {
                throw Error.ArgumentNull("loggers");
            }

            this.loggers = loggers.ToArray();
        }

        public IEnumerable<IExceptionLogger> Loggers
        {
            get { return this.loggers; }
        }

        public Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            List<Task> tasks = new List<Task>();

            Contract.Assert(this.loggers != null);

            foreach (IExceptionLogger logger in this.loggers)
            {
                if (logger == null)
                {
                    throw Error.InvalidOperation(Resources.TypeInstanceMustNotBeNull, typeof(IExceptionLogger).Name);
                }

                Task task = logger.LogAsync(context, cancellationToken);
                tasks.Add(task);
            }

            return Task.WhenAll(tasks);
        }
    }
}
