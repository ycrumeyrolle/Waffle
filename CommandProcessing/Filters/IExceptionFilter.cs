namespace CommandProcessing.Filters
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IExceptionFilter : IFilter
    {
        Task ExecuteExceptionFilterAsync(HandlerExecutedContext handlerExecutedContext, CancellationToken cancellationToken);
    }
}