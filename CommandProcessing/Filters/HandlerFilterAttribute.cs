namespace CommandProcessing.Filters
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;
    using CommandProcessing.Internal;
    using CommandProcessing.Tasks;

    /// <summary>
    /// Represents the base class for all handler-filter attributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public abstract class HandlerFilterAttribute : FilterAttribute, IHandlerFilter
    {
        /// <summary>
        /// Occurs before the handle method is invoked.
        /// </summary>
        /// <param name="handlerContext">The handler context.</param>
        public virtual void OnCommandExecuting(HandlerContext handlerContext)
        {
        }

        /// <summary>
        /// Occurs after the handle method is invoked.
        /// </summary>
        /// <param name="handlerExecutedContext">The handler executed context.</param>
        public virtual void OnCommandExecuted(HandlerExecutedContext handlerExecutedContext)
        {
        }

        /// <summary>
        /// Executes the filter action asynchronously.
        /// </summary>
        /// <typeparam name="TResult">The handler result type.</typeparam>
        /// <param name="handlerContext">The handler context.</param>
        /// <param name="cancellationToken">The cancellation token assigned for this task.</param>
        /// <param name="continuation">The delegate function to continue after the action method is invoked.</param>
        /// <returns>The newly created task for this operation.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to intercept all exceptions")]
        Task<TResult> IHandlerFilter.ExecuteHandlerFilterAsync<TResult>(HandlerContext handlerContext, CancellationToken cancellationToken, Func<Task<TResult>> continuation)
        {
            if (handlerContext == null)
            {
                throw Error.ArgumentNull("handlerContext");
            }

            if (continuation == null)
            {
                throw Error.ArgumentNull("continuation");
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
                    Tuple<object, Exception> tuple = this.CallOnHandlerExecuted<TResult>(handlerContext, response);
                    
                    if (tuple.Item1 == null)
                    {
                        return TaskHelpers.FromError<TResult>(tuple.Item2);
                    }

                    return TaskHelpers.FromResult((TResult)tuple.Item1);
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

                    Tuple<object, Exception> result = CallOnHandlerExecuted<TResult>(handlerContext, null, info.Exception);
                    return result.Item1 != null ? info.Handled((TResult)result.Item1) : info.Throw(result.Item2);
                 }, 
                cancellationToken);
        }

        private Tuple<object, Exception> CallOnHandlerExecuted<TResult>(HandlerContext handlerContext, object response = null, Exception exception = null)
        {
            Contract.Requires(handlerContext != null);
            Contract.Requires(response != null || exception != null);

            HandlerExecutedContext httpActionExecutedContext = new HandlerExecutedContext(handlerContext, exception)
            {
                Result = response
            };

            this.OnCommandExecuted(httpActionExecutedContext);
            if (httpActionExecutedContext.Result != null)
            {
                return new Tuple<object, Exception>(httpActionExecutedContext.Result, null);
            }

            if (httpActionExecutedContext.Exception != null)
            {
                return new Tuple<object, Exception>(default(TResult), httpActionExecutedContext.Exception);
            }

            throw Error.InvalidOperation(Resources.HandlerFilterAttribute_MustSupplyResponseOrException, this.GetType().Name);
        }
    }
}