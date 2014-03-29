namespace Waffle.Results
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Filters;
    using Waffle.Services;

    internal class CommandHandlerFilterResult : ICommandHandlerResult
    {
        private readonly CommandHandlerContext context;
        private readonly ServicesContainer services;
        private readonly ICommandHandlerFilter[] filters;

        public CommandHandlerFilterResult(CommandHandlerContext context, ServicesContainer services, ICommandHandlerFilter[] filters)
        {
            Contract.Requires(context != null);
            Contract.Requires(services != null);
            Contract.Requires(filters != null);

            this.context = context;
            this.services = services;
            this.filters = filters;
        }

        public async Task<HandlerResponse> ExecuteAsync(CancellationToken cancellationToken)
        {
            ActionInvoker actionInvoker = new ActionInvoker(this.context, cancellationToken, this.services);

            // Empty filters is the default case so avoid delegates
            // Ensure empty case remains the same as the filtered case
            if (this.filters.Length == 0)
            {
                return await actionInvoker.InvokeActionAsync();
            }

            // Ensure delegate continues to use the C# Compiler static delegate caching optimization
            Func<ActionInvoker, Task<HandlerResponse>> invokeCallback = innerInvoker => innerInvoker.InvokeActionAsync();
            return await InvokeActionWithActionFilters(this.context, cancellationToken, this.filters, invokeCallback, actionInvoker)();
        }

        public static Func<Task<HandlerResponse>> InvokeHandlerWithHandlerFiltersAsync(CommandHandlerContext context, CancellationToken cancellationToken, ICommandHandlerFilter[] filters, Func<Task<HandlerResponse>> innerAction)
        {
            Contract.Requires(context != null);
            Contract.Requires(filters != null);
            Contract.Requires(innerAction != null);

            Func<Task<HandlerResponse>> result = innerAction;
            for (int i = filters.Length - 1; i >= 0; i--)
            {
                ICommandHandlerFilter filter = filters[i];
                Func<Func<Task<HandlerResponse>>, ICommandHandlerFilter, Func<Task<HandlerResponse>>> chainContinuation = (continuation, innerFilter) => () => innerFilter.ExecuteHandlerFilterAsync(context, cancellationToken, continuation);

                result = chainContinuation(result, filter);
            }

            return result;
        }

        private static Func<Task<HandlerResponse>> InvokeActionWithActionFilters(CommandHandlerContext context, CancellationToken cancellationToken, ICommandHandlerFilter[] filters, Func<ActionInvoker, Task<HandlerResponse>> innerAction, ActionInvoker state)
        {
            return InvokeHandlerWithHandlerFiltersAsync(context, cancellationToken, filters, () => innerAction(state));
        }

        // Keep as struct to avoid allocation
        private struct ActionInvoker
        {
            private readonly CommandHandlerContext context;
            private readonly CancellationToken cancellationToken;
            private readonly ServicesContainer services;

            public ActionInvoker(CommandHandlerContext context, CancellationToken cancellationToken, ServicesContainer services)
            {
                Contract.Requires(services != null);

                this.context = context;
                this.cancellationToken = cancellationToken;
                this.services = services;
            }

            public Task<HandlerResponse> InvokeActionAsync()
            {
                return this.services.GetCommandHandlerInvoker().InvokeHandlerAsync(this.context, this.cancellationToken);
            }
        }
    }
}
