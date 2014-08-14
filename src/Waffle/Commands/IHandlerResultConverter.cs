namespace Waffle.Commands
{
    using System.Threading;
    using Waffle.Filters;
    using Waffle.Results;

    /// <summary>
    /// A contract for a conversion routine that can take the result of an handler returned from
    /// <see cref="CommandHandlerDescriptor.ExecuteAsync(CommandHandlerContext, CancellationToken)"/>
    /// and synchronously convert it to an instance of <see cref="HandlerResponse"/>.
    /// </summary>
    /// <remarks>This interface is not used when returning an <see cref="ICommandHandlerResult"/>.</remarks>
    public interface IHandlerResultConverter
    {
        /// <summary>
        /// Converts the result of an handler to an instance of <see cref="HandlerResponse"/>.
        /// </summary>
        /// <param name="context">The conversion context.</param>
        /// <param name="handlerResult">The result to convert.</param>
        /// <returns>A <see cref="HandlerResponse"/>.</returns>
        HandlerResponse Convert(CommandHandlerContext context, object handlerResult);
    }
}