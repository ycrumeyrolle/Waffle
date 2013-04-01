namespace CommandProcessing.Filters
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using CommandProcessing.Internal;
    using CommandProcessing.Tasks;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public abstract class ExceptionFilterAttribute : FilterAttribute, IExceptionFilter
    {
        public virtual void OnException(HandlerExecutedContext actionExecutedContext)
        {
        }

        Task IExceptionFilter.ExecuteExceptionFilterAsync(HandlerExecutedContext handlerExecutedContext, CancellationToken cancellationToken)
        {
            if (handlerExecutedContext == null)
            {
                throw Error.ArgumentNull("handlerExecutedContext");
            }

            this.OnException(handlerExecutedContext);
            return TaskHelpers.Completed();
        }
    }
}
