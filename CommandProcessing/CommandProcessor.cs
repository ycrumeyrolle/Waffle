namespace CommandProcessing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using CommandProcessing.Dispatcher;
    using CommandProcessing.Filters;
    using CommandProcessing.Services;
    using CommandProcessing.Validation;

    public class CommandProcessor : ICommandProcessor, IDisposable
    {
        private bool disposed;

        public CommandProcessor()
            : this(new ProcessorConfiguration())
        {
        }

        public CommandProcessor(ProcessorConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            this.Configuration = configuration;

            this.Initialize();
        }

        public ProcessorConfiguration Configuration { get; private set; }

        public IHandlerSelector HandlerSelector { get; private set; }
        
        public void Process<TCommand>(TCommand command) where TCommand : ICommand
        {
            this.Process<TCommand, EmptyResult>(command);
        }
        
        public TResult Process<TCommand, TResult>(TCommand command) where TCommand : ICommand
        {
            IEnumerable<ICommandValidator> validators = this.Configuration.Services.GetCommandValidators();
            bool valid = validators
                .Select(validator => validator.Validate(command))
                .Aggregate(true, (current, commandValid) => current & commandValid);

            if (!valid && this.Configuration.AbortOnInvalidCommand)
            {
                return default(TResult);
            }

            using (HandlerRequest request = new HandlerRequest(this.Configuration, command))
            {
                HandlerDescriptor descriptor = this.HandlerSelector.SelectHandler(request);

                Handler handler = descriptor.CreateHandler(request);

                if (handler == null)
                {
                    throw new HandlerNotFoundException(typeof(TCommand));
                }

                HandlerContext context = new HandlerContext(request, descriptor);

                IEnumerable<FilterInfo> filterPipeline = descriptor.GetFilterPipeline();

                FilterGrouping filterGrouping = new FilterGrouping(filterPipeline);
                try
                {
                    HandlerExecutedContext executedContext = this.InvokeHandlerWithFilters(context, filterGrouping.HandlerFilters, handler);
                    return (TResult)executedContext.Result;
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    ExceptionContext exceptionContext = this.InvokeExceptionFilters(context, filterGrouping.ExceptionFilters, exception);

                    if (!exceptionContext.ExceptionHandled)
                    {
                        throw;
                    }

                    return (TResult)exceptionContext.Result;
                }
            }
        }

        public TService Using<TService>() where TService : class
        {
            var service = this.Configuration.DependencyResolver.GetServiceOrThrow<TService>();

            if (this.Configuration.ServiceProxyCreationEnabled)
            {
                var proxyBuilder = this.Configuration.Services.GetProxyBuilder();
                var interceptorProvider = this.Configuration.Services.GetInterceptorProvider();

                service = proxyBuilder.Build(service, interceptorProvider);
            }

            return service;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static HandlerExecutedContext InvokeCommandFilter(IHandlerFilter filter, HandlerExecutingContext preContext, Func<HandlerExecutedContext> continuation)
        {
            filter.OnCommandExecuting(preContext);
            if (preContext.Result != null && preContext.Result != null)
            {
                return new HandlerExecutedContext(preContext, true, null) { Result = preContext.Result };
            }

            bool exceptionOccured = false;
            HandlerExecutedContext actionExecutedContext;
            try
            {
                actionExecutedContext = continuation();
            }
            catch (ThreadAbortException)
            {
                actionExecutedContext = new HandlerExecutedContext(preContext, false, null);
                filter.OnCommandExecuted(actionExecutedContext);
                throw;
            }
            catch (Exception exception)
            {
                exceptionOccured = true;
                actionExecutedContext = new HandlerExecutedContext(preContext, false, exception);
                filter.OnCommandExecuted(actionExecutedContext);
                if (!actionExecutedContext.ExceptionHandled)
                {
                    throw;
                }
            }

            if (!exceptionOccured)
            {
                filter.OnCommandExecuted(actionExecutedContext);
            }

            return actionExecutedContext;
        }

        protected virtual ExceptionContext InvokeExceptionFilters(HandlerContext context, IEnumerable<IExceptionFilter> filters, Exception exception)
        {
            ExceptionContext exceptionContext = new ExceptionContext(context, exception);
            foreach (IExceptionFilter current in filters.Reverse())
            {
                current.OnException(exceptionContext);
            }

            return exceptionContext;
        }

        protected virtual HandlerExecutedContext InvokeHandlerWithFilters(HandlerContext context, IEnumerable<IHandlerFilter> filters, Handler handler)
        {
            HandlerExecutingContext preContext = new HandlerExecutingContext(context);
            Func<HandlerExecutedContext> seed = () => new HandlerExecutedContext(preContext, false, null) { Result = this.InvokeHandler(context, handler) };
            Func<HandlerExecutedContext> func = filters.Reverse().Aggregate(seed, (next, filter) => () => CommandProcessor.InvokeCommandFilter(filter, preContext, next));
            return func();
        }

        protected virtual object InvokeHandler(HandlerContext context, Handler handler)
        {
            object result = handler.Handle(context.Command);

            return result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.disposed = true;
                if (disposing)
                {
                    this.Configuration.Dispose();
                }
            }
        }

        private void Initialize()
        {
            this.Configuration.Initializer(this.Configuration);
            this.HandlerSelector = this.Configuration.Services.GetHandlerSelector();
        }
    }
}