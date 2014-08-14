namespace Waffle.Results
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Events;
    using Waffle.Services;

    internal class EventHandlerFilterResult : IEventHandlerResult
    {
        private readonly EventHandlerContext context;
        private readonly ServicesContainer services;
        private readonly IEventHandlerFilter[] filters;

        public EventHandlerFilterResult(EventHandlerContext context, ServicesContainer services, IEventHandlerFilter[] filters)
        {
            Contract.Requires(context != null);
            Contract.Requires(services != null);
            Contract.Requires(filters != null);

            this.context = context;
            this.services = services;
            this.filters = filters;
        }

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            ActionInvoker actionInvoker = new ActionInvoker(this.context, cancellationToken, this.services);

            // Empty filters is the default case so avoid delegates
            // Ensure empty case remains the same as the filtered case
            if (this.filters.Length == 0)
            {
                return actionInvoker.InvokeActionAsync();
            }

            // Ensure delegate continues to use the C# Compiler static delegate caching optimization
            Func<ActionInvoker, Task> invokeCallback = innerInvoker => innerInvoker.InvokeActionAsync();
            return InvokeActionWithActionFilters(this.context, cancellationToken, this.filters, invokeCallback, actionInvoker)();
        }

        public static Func<Task> InvokeHandlerWithHandlerFiltersAsync(EventHandlerContext context, CancellationToken cancellationToken, IEventHandlerFilter[] filters, Func<Task> innerAction)
        {
            Contract.Requires(context != null);
            Contract.Requires(filters != null);
            Contract.Requires(innerAction != null);

            Func<Task> result = innerAction;
            for (int i = filters.Length - 1; i >= 0; i--)
            {
                IEventHandlerFilter commandFilter = filters[i];
                Func<Func<Task>, IEventHandlerFilter, Func<Task>> chainContinuation = (continuation, innerFilter) => () => innerFilter.ExecuteHandlerFilterAsync(context, cancellationToken, continuation);

                result = chainContinuation(result, commandFilter);
            }

            return result;
        }

        private static Func<Task> InvokeActionWithActionFilters(EventHandlerContext context, CancellationToken cancellationToken, IEventHandlerFilter[] filters, Func<ActionInvoker, Task> innerAction, ActionInvoker state)
        {
            return InvokeHandlerWithHandlerFiltersAsync(context, cancellationToken, filters, () => innerAction(state));
        }

        // Keep as struct to avoid allocation
        private struct ActionInvoker
        {
            private readonly EventHandlerContext context;
            private readonly CancellationToken cancellationToken;
            private readonly ServicesContainer services;

            public ActionInvoker(EventHandlerContext context, CancellationToken cancellationToken, ServicesContainer services)
            {
                Contract.Requires(services != null);

                this.context = context;
                this.cancellationToken = cancellationToken;
                this.services = services;
            }

            public Task InvokeActionAsync()
            {
                return this.services.GetEventHandlerInvoker().InvokeHandlerAsync(this.context, this.cancellationToken);
            }
        }
    }
}
