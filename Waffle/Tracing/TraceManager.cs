namespace Waffle.Tracing
{
    using Waffle.Commands;
    using Waffle.Events;
    using Waffle.Internal;
    using Waffle.Services;
    using Waffle.Validation;

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
            CreateCommandHandlerSelectorTracer(configuration, traceWriter);
            CreateCommandHandlerActivatorTracer(configuration, traceWriter);
            CreateHandlerValidatorTracer(configuration, traceWriter);
            CreateCommandWorkerTracer(configuration, traceWriter);

            CreateEventHandlerSelectorTracer(configuration, traceWriter);
            CreateEventHandlerActivatorTracer(configuration, traceWriter);
            CreateEventWorkerTracer(configuration, traceWriter);
        }

        // Get services from the global config. These are normally per-handler services, but we're getting the global fallbacks.
        private static TService GetService<TService>(ServicesContainer services)
        {
            return (TService)services.GetService(typeof(TService));
        }

        private static void CreateCommandHandlerSelectorTracer(ProcessorConfiguration configuration, ITraceWriter traceWriter)
        {
            ICommandHandlerSelector selector = GetService<ICommandHandlerSelector>(configuration.Services);
            if (selector != null && !(selector is CommandHandlerSelectorTracer))
            {
                CommandHandlerSelectorTracer tracer = new CommandHandlerSelectorTracer(selector, traceWriter);
                configuration.Services.Replace(typeof(ICommandHandlerSelector), tracer);
            }
        }

        private static void CreateCommandHandlerActivatorTracer(ProcessorConfiguration configuration, ITraceWriter traceWriter)
        {
            ICommandHandlerActivator activator = GetService<ICommandHandlerActivator>(configuration.Services);
            if (activator != null && !(activator is CommandHandlerActivatorTracer))
            {
                CommandHandlerActivatorTracer tracer = new CommandHandlerActivatorTracer(activator, traceWriter);
                configuration.Services.Replace(typeof(ICommandHandlerActivator), tracer);
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
        
        private static void CreateEventHandlerSelectorTracer(ProcessorConfiguration configuration, ITraceWriter traceWriter)
        {
            IEventHandlerSelector selector = GetService<IEventHandlerSelector>(configuration.Services);
            if (selector != null && !(selector is EventHandlerSelectorTracer))
            {
                EventHandlerSelectorTracer tracer = new EventHandlerSelectorTracer(selector, traceWriter);
                configuration.Services.Replace(typeof(IEventHandlerSelector), tracer);
            }
        }

        private static void CreateEventHandlerActivatorTracer(ProcessorConfiguration configuration, ITraceWriter traceWriter)
        {
            IEventHandlerActivator activator = GetService<IEventHandlerActivator>(configuration.Services);
            if (activator != null && !(activator is EventHandlerActivatorTracer))
            {
                EventHandlerActivatorTracer tracer = new EventHandlerActivatorTracer(activator, traceWriter);
                configuration.Services.Replace(typeof(IEventHandlerActivator), tracer);
            }
        }

        private static void CreateEventWorkerTracer(ProcessorConfiguration configuration, ITraceWriter traceWriter)
        {
            IEventWorker worker = GetService<IEventWorker>(configuration.Services);
            if (worker != null && !(worker is EventWorkerTracer))
            {
                EventWorkerTracer tracer = new EventWorkerTracer(worker, traceWriter);
                configuration.Services.Replace(typeof(IEventWorker), tracer);
            }
        }
    }
}