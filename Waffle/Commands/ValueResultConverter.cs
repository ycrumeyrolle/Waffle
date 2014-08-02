//#if LOOSE_CQRS
namespace Waffle.Commands
{
    using Waffle.Filters;
    using Waffle.Internal;

    /// <summary>
    /// A converter for creating responses from actions that return an arbitrary T value.
    /// </summary>
    /// <typeparam name="T">The declared return type of an action.</typeparam>
    internal class ValueResultConverter<T> : IHandlerResultConverter
    {
        public HandlerResponse Convert(CommandHandlerContext context, object handlerResult)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            HandlerResponse resultAsResponse = handlerResult as HandlerResponse;
            if (resultAsResponse != null)
            {
                HandlerResponse.EnsureResponseHasRequest(resultAsResponse, context.Request);
                return resultAsResponse;
            }

            T value = (T)handlerResult;
            return context.Request.CreateResponse(/*value*/);
        }
    }
}
//#endif