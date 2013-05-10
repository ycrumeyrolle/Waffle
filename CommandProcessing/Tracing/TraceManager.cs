namespace CommandProcessing.Tracing
{
    using CommandProcessing.Dispatcher;
    using CommandProcessing.Internal;
    using CommandProcessing.Services;
    using CommandProcessing.Validation;

    internal class TraceManager : ITraceManager
    {
        public void Initialize(ProcessorConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            ITraceWriter traceWriter = configuration.Services.GetTraceWriter();
            if (traceWriter != null)
            {
                // Install tracers only when a custom trace writer has been registered
                CreateAllTracers(configuration, traceWriter);
            }
        }

        private static void CreateAllTracers(ProcessorConfiguration configuration, ITraceWriter traceWriter)
        {
            CreateHandlerSelectorTracer(configuration, traceWriter);
            CreateHandlerActivatorTracer(configuration, traceWriter);
            CreateHandlerValidatorTracer(configuration, traceWriter);
            CreateCommandWorkerTracer(configuration, traceWriter);
        }

        // Get services from the global config. These are normally per-handler services, but we're getting the global fallbacks.
        private static TService GetService<TService>(ServicesContainer services)
        {
            return (TService)services.GetService(typeof(TService));
        }

        private static void CreateHandlerSelectorTracer(ProcessorConfiguration configuration, ITraceWriter traceWriter)
        {
            IHandlerSelector selector = GetService<IHandlerSelector>(configuration.Services);
            if (selector != null && !(selector is HandlerSelectorTracer))
            {
                HandlerSelectorTracer tracer = new HandlerSelectorTracer(selector, traceWriter);
                configuration.Services.Replace(typeof(IHandlerSelector), tracer);
            }
        }

        private static void CreateHandlerActivatorTracer(ProcessorConfiguration configuration, ITraceWriter traceWriter)
        {
            IHandlerActivator activator = GetService<IHandlerActivator>(configuration.Services);
            if (activator != null && !(activator is HandlerActivatorTracer))
            {
                HandlerActivatorTracer tracer = new HandlerActivatorTracer(activator, traceWriter);
                configuration.Services.Replace(typeof(IHandlerActivator), tracer);
            }
        }

        private static void CreateHandlerValidatorTracer(ProcessorConfiguration configuration, ITraceWriter traceWriter)
        {
            ICommandValidator activator = GetService<ICommandValidator>(configuration.Services);
            if (activator != null && !(activator is CommandValidatorTracer))
            {
                CommandValidatorTracer tracer = new CommandValidatorTracer(activator, traceWriter);
                configuration.Services.Replace(typeof(ICommandValidator), tracer);
            }
        }

        private static void CreateCommandWorkerTracer(ProcessorConfiguration configuration, ITraceWriter traceWriter)
        {
            ICommandWorker worker = GetService<ICommandWorker>(configuration.Services);
            if (worker != null && !(worker is CommandWorkerTracer))
            {
                CommandWorkerTracer tracer = new CommandWorkerTracer(worker, traceWriter);
                configuration.Services.Replace(typeof(ICommandWorker), tracer);
            }
        }
    }
}