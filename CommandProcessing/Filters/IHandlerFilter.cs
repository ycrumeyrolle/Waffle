namespace CommandProcessing.Filters
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IHandlerFilter : IFilter
    {
        Task<TResult> ExecuteHandlerFilterAsync<TResult>(HandlerContext handlerContext, CancellationToken cancellationToken, Func<Task<TResult>> continuation);
    }
}