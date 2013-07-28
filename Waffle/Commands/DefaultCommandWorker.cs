namespace Waffle.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Filters;
    using Waffle.Internal;
    using Waffle.Tasks;
    using Waffle.Validation;

    /// <summary>
    /// Default implementation of the <see cref="ICommandWorker"/>.
    /// </summary>
    public sealed class DefaultCommandWorker : ICommandWorker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCommandWorker"/> class.
        /// </summary>
        /// <param name="configuration"></param>
        public DefaultCommandWorker(ProcessorConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public ProcessorConfiguration Configuration { get; private set; }

        /// <summary>
        /// Execute the request via the worker. 
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="request">The <see cref="HandlerRequest"/> to execute.</param>
        /// <returns>The result of the command, if any.</returns>
        public Task<TResult> ExecuteAsync<TResult>(CommandHandlerRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            ICommandHandlerSelector handlerSelector = this.Configuration.Services.GetHandlerSelector();
            CommandHandlerDescriptor descriptor = handlerSelector.SelectHandler(request);

            ICommandHandler commandHandler = descriptor.CreateHandler(request);

            if (commandHandler == null)
            {
                throw CreateHandlerNotFoundException(descriptor);
            }

            this.RegisterForDispose(request, descriptor.Lifetime, commandHandler);

            if (!this.ValidateCommand(request) && this.Configuration.AbortOnInvalidCommand)
            {
                return TaskHelpers.Completed<TResult>();
            }

            CommandHandlerContext context = new CommandHandlerContext(request, descriptor);

            CommandFilterGrouping commandFilterGrouping = descriptor.GetFilterGrouping();
            CancellationToken cancellationToken = new CancellationToken();

            Func<Task<object>> invokeFunc = InvokeHandlerWithHandlerFiltersAsync(context, cancellationToken, commandFilterGrouping.CommandHandlerFilters, () => InvokeHandlerAsync(context, commandHandler, cancellationToken));
            Task<object> result = invokeFunc();

            result = InvokeHandlerWithExceptionFiltersAsync(result, context, cancellationToken, commandFilterGrouping.ExceptionFilters);

            return result.CastFromObject<TResult>();
        }

        private void RegisterForDispose(HandlerRequest request, HandlerLifetime lifetime, ICommandHandler commandHandler)
        {
            if (lifetime == HandlerLifetime.Processor)
            {
                // Per-processor lifetime will be disposed only on processor disposing
                this.Configuration.RegisterForDispose(commandHandler as IDisposable);
            }
            else
            {
                // Per-request and transcient lifetime will be disposed on main request disposing
                request.RegisterForDispose(commandHandler, true);
            }
        }

        private bool ValidateCommand(CommandHandlerRequest request)
        {
            ICommandValidator validator = this.Configuration.Services.GetCommandValidator();
            bool valid = validator.Validate(request);

            return valid;
        }

        private static Task<TResult> InvokeHandlerWithExceptionFiltersAsync<TResult>(Task<TResult> task, CommandHandlerContext context, CancellationToken cancellationToken, IEnumerable<IExceptionFilter> filters)
        {
            Contract.Requires(task != null);
            Contract.Requires(context != null);
            Contract.Requires(filters != null);

            return task.Catch(
                info =>
                {
                    HandlerExecutedContext executedContext = new HandlerExecutedContext(context, info.Exception);

                    // Note: exception filters need to be scheduled in the reverse order so that
                    // the more specific filter (e.g. Handler) executes before the less specific ones (e.g. Global)
                    filters = filters.Reverse();

                    // Note: in order to work correctly with the TaskHelpers.Iterate method, the lazyTaskEnumeration
                    // must be lazily evaluated. Otherwise all the tasks might start executing even though we want to run them
                    // sequentially and not invoke any of the following ones if an earlier fails.
                    IEnumerable<Task> lazyTaskEnumeration = filters.Select(filter => filter.ExecuteExceptionFilterAsync(executedContext, cancellationToken));
                    Task<TResult> resultTask = TaskHelpers.Iterate(lazyTaskEnumeration, cancellationToken)
                                   .Then(
                                   () =>
                                   {
                                       if (executedContext.Result != null)
                                       {
                                           return TaskHelpers.FromResult((TResult)executedContext.Result);
                                       }

                                       return TaskHelpers.FromError<TResult>(executedContext.Exception);
                                   },
                                    runSynchronously: true);

                    return info.Task(resultTask);
                });
        }

        private static Func<Task<TResult>> InvokeHandlerWithHandlerFiltersAsync<TResult>(CommandHandlerContext context, CancellationToken cancellationToken, ICommandHandlerFilter[] filters, Func<Task<TResult>> innerAction)
        {
            Contract.Requires(context != null);
            Contract.Requires(filters != null);
            Contract.Requires(innerAction != null);

            Func<Task<TResult>> result = innerAction;
            for (int i = filters.Length - 1; i >= 0; i--)
            {
                ICommandHandlerFilter filter = filters[i];
                Func<Func<Task<TResult>>, ICommandHandlerFilter, Func<Task<TResult>>> chainContinuation = (continuation, innerFilter) =>
                {
                    return () => innerFilter.ExecuteHandlerFilterAsync(context, cancellationToken, continuation);
                };

                result = chainContinuation(result, filter);
            }

            return result;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception type is reflected into a faulted task.")]
        private static Task<object> InvokeHandlerAsync(CommandHandlerContext context, ICommandHandler commandHandler, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskHelpers.Canceled<object>();
            }

            try
            {
                var result = commandHandler.Handle(context.Command, context);
                return TaskHelpers.FromResult(result);
            }
            catch (Exception e)
            {
                return TaskHelpers.FromError<object>(e);
            }
        }

        private static CommandHandlerNotFoundException CreateHandlerNotFoundException(CommandHandlerDescriptor descriptor)
        {
            if (descriptor.ResultType == typeof(VoidResult))
            {
                return new CommandHandlerNotFoundException(descriptor.MessageType);
            }

            return new CommandHandlerNotFoundException(descriptor.MessageType, descriptor.ResultType);
        }
    }
}