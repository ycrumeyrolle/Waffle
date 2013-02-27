namespace CommandProcessing.Filters
{
    public interface IExceptionFilter : IFilter
    {
        void OnException(ExceptionContext exceptionContext);
    }
}