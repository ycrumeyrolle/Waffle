namespace Waffle.Commands
{
    using System.Diagnostics.Contracts;
    using Waffle.Filters;
    using Waffle.Internal;

    /// <summary>
    /// A converter for creating a response from actions that do not return a value.
    /// </summary>
    internal class VoidResultConverter : IHandlerResultConverter
    {
        public HandlerResponse Convert(CommandHandlerContext context, object handlerResult)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            Contract.Assert(handlerResult == null);
            return context.Request.CreateResponse(default(VoidResult));
        }
    }
}