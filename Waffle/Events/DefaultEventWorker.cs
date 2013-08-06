namespace Waffle.Events
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Commands;
    using Waffle.Filters;
    using Waffle.Internal;
    using Waffle.Tasks;

    /// <summary>
    /// Default implementation of the <see cref="IEventWorker"/>.
    /// </summary>
    public sealed class DefaultEventWorker : IEventWorker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultEventWorker"/> class.
        /// </summary>
        /// <param name="configuration"></param>
        public DefaultEventWorker(ProcessorConfiguration configuration)
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
        /// <param name="request">The <see cref="EventHandlerRequest"/> to execute.</param>
        /// <returns>The <see cref="Task"/> of the event.</returns>
        public Task PublishAsync(EventHandlerRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            IEventHandlerSelector handlerSelector = this.Configuration.Services.GetEventHandlerSelector();

            EventHandlersDescriptor eventDescriptor = handlerSelector.SelectHandlers(request);
            IEnumerable<Task> invokeHandlerTasks = eventDescriptor.EventHandlerDescriptors.Select(descriptor => this.InvokeHandlerAsync(descriptor, request, cancellationToken));
            Task result = TaskHelpers.Iterate(invokeHandlerTasks, cancellationToken);

            return result;
        }

        private Task InvokeHandlerAsync(EventHandlerDescriptor descriptor, EventHandlerRequest request, CancellationToken cancellationToken)
        {
            IEventHandler handler = descriptor.CreateHandler(request);
            if (handler == null)
            {
                throw CreateHandlerNotFoundException(descriptor);
            }

            this.RegisterForDispose(request, descriptor.Lifetime, handler);
            EventHandlerContext context = new EventHandlerContext(request, descriptor);
            EventFilterGrouping filterGrouping = descriptor.GetFilterGrouping();

            Func<Task> invokeFunc = InvokeHandlerWithHandlerFiltersAsync(context, cancellationToken, filterGrouping.EventHandlerFilters, () => InvokeHandlerAsync(handler, context, cancellationToken));
            Task result = descriptor.RetryPolicy.ExecuteAsync(invokeFunc, cancellationToken);
            return result;
        }

        private static Func<Task> InvokeHandlerWithHandlerFiltersAsync(EventHandlerContext context, CancellationToken cancellationToken, IEventHandlerFilter[] eventFilters, Func<Task> innerAction)
        {
            Contract.Requires(context != null);
            Contract.Requires(eventFilters != null);
            Contract.Requires(innerAction != null);

            Func<Task> result = innerAction;
            for (int i = eventFilters.Length - 1; i >= 0; i--)
            {
                IEventHandlerFilter commandFilter = eventFilters[i];
                Func<Func<Task>, IEventHandlerFilter, Func<Task>> chainContinuation = (continuation, innerFilter) =>
                {
                    return () => innerFilter.ExecuteHandlerFilterAsync(context, cancellationToken, continuation);
                };

                result = chainContinuation(result, commandFilter);
            }

            return result;
        }

        private void RegisterForDispose(HandlerRequest request, HandlerLifetime lifetime, IEventHandler handler)
        {
            if (lifetime == HandlerLifetime.Processor)
            {
                // Per-processor lifetime will be disposed only on processor disposing
                this.Configuration.RegisterForDispose(handler as IDisposable);
            }
            else
            {
                // Per-request and transcient lifetime will be disposed on main request disposing
                request.RegisterForDispose(handler, true);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception type is reflected into a faulted task.")]
        private static Task InvokeHandlerAsync(IEventHandler handler, EventHandlerContext context, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskHelpers.Canceled();
            }

            try
            {
                handler.Handle(context.Event, context);
            }
            catch (Exception e)
            {
                return TaskHelpers.FromError(e);
            }

            return TaskHelpers.Completed();
        }

        private static CommandHandlerNotFoundException CreateHandlerNotFoundException(EventHandlerDescriptor descriptor)
        {
            return new CommandHandlerNotFoundException(descriptor.MessageType);
        }
    }
}