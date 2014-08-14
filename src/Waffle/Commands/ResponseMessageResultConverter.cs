namespace Waffle.Commands
{
    using System;
    using Waffle.Filters;
    using Waffle.Internal;
    using Waffle.Properties;

    /// <summary>
    /// A converter for actions with a return type of <see cref="HandlerResponse"/>.
    /// </summary>
    public class ResponseMessageResultConverter : IHandlerResultConverter
    {
        /// <summary>
        /// Converts the result of an handler to an instance of <see cref="HandlerResponse"/>.
        /// </summary>
        /// <param name="context">The conversion context.</param>
        /// <param name="handlerResult">The result to convert.</param>
        /// <returns>A <see cref="HandlerResponse"/>.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public HandlerResponse Convert(CommandHandlerContext context, object handlerResult)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            HandlerResponse response = (HandlerResponse)handlerResult;
            if (response == null)
            {
                throw Error.InvalidOperation(Resources.DefaultHandlerInvoker_NullHandlerResponse);
            }

            HandlerResponse.EnsureResponseHasRequest(response, context.Request);
            return response;
        }
    }
}