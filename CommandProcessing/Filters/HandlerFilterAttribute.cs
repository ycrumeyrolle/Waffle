namespace CommandProcessing.Filters
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;
    using CommandProcessing.Tasks;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public abstract class HandlerFilterAttribute : FilterAttribute, IHandlerFilter
    {
        public virtual void OnCommandExecuting(HandlerContext handlerContext)
        {
        }

        public virtual void OnCommandExecuted(HandlerExecutedContext handlerExecutedContext)
        {
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to intercept all exceptions")]
        Task<TResult> IHandlerFilter.ExecuteHandlerFilterAsync<TResult>(HandlerContext handlerContext, CancellationToken cancellationToken, Func<Task<TResult>> continuation)
        {
            if (handlerContext == null)
            {
                throw new ArgumentNullException("handlerContext");
            }

            if (continuation == null)
            {
                throw new ArgumentNullException("continuation");
            }

            try
            {
                this.OnCommandExecuting(handlerContext);
            }
            catch (Exception exception)
            {
                return TaskHelpers.FromError<TResult>(exception);
            }

            if (handlerContext.Result != null)
            {
                return TaskHelpers.FromResult((TResult)handlerContext.Result);
            }

            Task<TResult> task = continuation();
            bool calledOnActionExecuted = false;

            return task.Then(
                response =>
                {
                    calledOnActionExecuted = true;
                    Tuple<TResult, Exception> tuple = this.CallOnHandlerExecuted<TResult>(handlerContext, response);
                    
                    // TODO : encaspulates result into reference type (Result for example)
                    if (tuple.Item1 == null)
                    {
                        return TaskHelpers.FromError<TResult>(tuple.Item2);
                    }

                    return TaskHelpers.FromResult(tuple.Item1);
                }, 
                cancellationToken).Catch(
                info =>
                {
                    // If we've already called OnActionExecuted, that means this catch is running because
                    // OnActionExecuted threw an exception, so we just want to re-throw the exception rather
                    // that calling OnActionExecuted again. We also need to reset the response to forget about it
                    // since a filter threw an exception.
                    if (calledOnActionExecuted)
                    {
                        handlerContext.Result = null;
                        return info.Throw();
                    }

                    // TODO : encaspulates result into reference type (Result for example)
                    Tuple<TResult, Exception> result = CallOnHandlerExecuted<TResult>(handlerContext, null, info.Exception);
                    return result.Item1 != null ? info.Handled(result.Item1) : info.Throw(result.Item2);
                 }, 
                cancellationToken);
        }

        private Tuple<TResult, Exception> CallOnHandlerExecuted<TResult>(HandlerContext handlerContext, object response = null, Exception exception = null)
        {
            Contract.Assert(handlerContext != null);
            Contract.Assert(response != null || exception != null);

            HandlerExecutedContext httpActionExecutedContext = new HandlerExecutedContext(handlerContext, exception)
            {
                Result = response
            };

            this.OnCommandExecuted(httpActionExecutedContext);
            if (httpActionExecutedContext.Result != null)
            {
                return new Tuple<TResult, Exception>((TResult)httpActionExecutedContext.Result, null);
            }

            if (httpActionExecutedContext.Exception != null)
            {
                return new Tuple<TResult, Exception>(default(TResult), httpActionExecutedContext.Exception);
            }

            throw new InvalidOperationException(string.Format(Resources.HandlerFilterAttribute_MustSupplyResponseOrException, this.GetType().Name));
        }
    }
}