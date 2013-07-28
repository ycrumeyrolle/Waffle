namespace Waffle.Tracing
{
    using System.Diagnostics.Contracts;
    using Waffle.Commands;
    using Waffle.Filters;
    using Waffle.Properties;

    /// <summary>
    /// Tracer for <see cref="ICommandHandlerActivator"/>.
    /// </summary>
    internal class CommandHandlerActivatorTracer : ICommandHandlerActivator, IDecorator<ICommandHandlerActivator>
    {
        private const string CreateMethodName = "Create";

        private readonly ICommandHandlerActivator innerActivator;
        private readonly ITraceWriter traceWriter;

        public CommandHandlerActivatorTracer(ICommandHandlerActivator innerActivator, ITraceWriter traceWriter)
        {
            Contract.Assert(innerActivator != null);
            Contract.Assert(traceWriter != null);

            this.innerActivator = innerActivator;
            this.traceWriter = traceWriter;
        }

        public ICommandHandlerActivator Inner
        {
            get { return this.innerActivator; }
        }

        ICommandHandler ICommandHandlerActivator.Create(HandlerRequest request, HandlerDescriptor descriptor)
        {
            ICommandHandler commandHandler = null;

            this.traceWriter.TraceBeginEnd(
                request,
                TraceCategories.HandlersCategory,
                TraceLevel.Info,
                this.innerActivator.GetType().Name,
                CreateMethodName,
                beginTrace: null,
                execute: () => commandHandler = this.innerActivator.Create(request, descriptor),
                endTrace: tr => tr.Message = commandHandler == null ? Resources.TraceNoneObjectMessage : commandHandler.GetType().FullName,
                errorTrace: null);

            if (commandHandler != null && !(commandHandler is CommandHandlerTracer))
            {
                commandHandler = new CommandHandlerTracer(request, commandHandler, this.traceWriter);
            }

            return commandHandler;
        }
    }
}