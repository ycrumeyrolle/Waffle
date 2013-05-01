namespace CommandProcessing.Filters
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the methods that are used in an handler filter.
    /// </summary>
    public interface IHandlerFilter : IFilter
    {
        /// <summary>
        /// Executes the filter handler asynchronously.
        /// </summary>
        /// <typeparam name="TResult">The handler result type.</typeparam>
        /// <param name="handlerContext">The handler context.</param>
        /// <param name="cancellationToken">The cancellation token assigned for this task.</param>
        /// <param name="continuation">The delegate function to continue after the handler method is invoked.</param>
        /// <returns>The newly created task for this operation.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "It is only a two level nesting.")]
        Task<TResult> ExecuteHandlerFilterAsync<TResult>(HandlerContext handlerContext, CancellationToken cancellationToken, Func<Task<TResult>> continuation);
    }
}