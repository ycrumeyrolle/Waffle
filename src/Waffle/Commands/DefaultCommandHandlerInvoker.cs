namespace Waffle.Commands
{
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Filters;
    using Waffle.Internal;
    using Waffle.Properties;
    using Waffle.Results;

    /// <summary>
    /// Default implementation of the <see cref="ICommandHandlerInvoker"/> interface.
    /// </summary>
    public class DefaultCommandHandlerInvoker : ICommandHandlerInvoker
    {
        /// <summary>
        /// Invokes a <see cref="ICommandHandler"/>.
        /// </summary>
        /// <param name="context">The context of invocation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> of HandlerResponse.</returns>
        public virtual Task<HandlerResponse> InvokeHandlerAsync(CommandHandlerContext context, CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            return InvokeActionAsyncCore(context, cancellationToken);
        }

        private static async Task<HandlerResponse> InvokeActionAsyncCore(CommandHandlerContext context, CancellationToken cancellationToken)
        {
            Contract.Requires(context != null);
            Contract.Requires(context.Descriptor != null);

            CommandHandlerDescriptor handlerDescriptor = context.Descriptor;
            
            try
            {
                object result = await handlerDescriptor.ExecuteAsync(context, cancellationToken);

                // This is cached in a local for performance reasons. ReturnType is a virtual property on CommandHandlerDescriptor,
                // or else we'd want to cache this as part of that class.
                bool isDeclaredTypeHandlerResult = typeof(ICommandHandlerResult).IsAssignableFrom(handlerDescriptor.ReturnType);
                if (result == null && isDeclaredTypeHandlerResult)
                {
                    // If the return type of the action descriptor is IHandlerResult, it's not valid to return null
                    throw Error.InvalidOperation(Resources.DefaultHandlerInvoker_NullHandlerResult);
                }

                if (isDeclaredTypeHandlerResult || handlerDescriptor.ReturnType == typeof(object))
                {
                    ICommandHandlerResult actionResult = result as ICommandHandlerResult;

                    if (actionResult == null && isDeclaredTypeHandlerResult)
                    {
                        // If the return type of the action descriptor is IHandlerResult, it's not valid to return an
                        // object that doesn't implement IHandlerResult
                        throw Error.InvalidOperation(Resources.DefaultHandlerInvoker_InvalidHandlerResult, result.GetType());
                    }

                    if (actionResult != null)
                    {
                        HandlerResponse response = await actionResult.ExecuteAsync(cancellationToken);
                        if (response == null)
                        {
                            throw Error.InvalidOperation(Resources.DefaultHandlerInvoker_NullHandlerResponse);
                        }

                        HandlerResponse.EnsureResponseHasRequest(response, context.Request);
                        return response;
                    }
                }

                // This is a non-IHandlerResult, so run the converter
                return handlerDescriptor.ResultConverter.Convert(context, result);
            }
            catch (HandlerResponseException handlerResponseException)
            {
                HandlerResponse response = handlerResponseException.Response;
                HandlerResponse.EnsureResponseHasRequest(response, context.Request);

                return response;
            }
        }
    }
}