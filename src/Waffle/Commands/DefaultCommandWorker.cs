namespace Waffle.Commands
{
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;
    using Waffle.ExceptionHandling;
    using Waffle.Filters;
    using Waffle.Internal;
    using Waffle.Results;
    using Waffle.Retrying;
    using Waffle.Services;

    /// <summary>
    /// Default implementation of the <see cref="ICommandWorker"/>.
    /// </summary>
    public sealed class DefaultCommandWorker : ICommandWorker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCommandWorker"/> class.
        /// </summary>
        /// <param name="configuration"></param>
        public DefaultCommandWorker(ProcessorConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public ProcessorConfiguration Configuration { get; private set; }

        /// <summary>
        /// Execute the request via the worker. 
        /// </summary>
        /// <param name="request">The <see cref="HandlerRequest"/> to execute.</param>
        /// <returns>The result of the command, if any.</returns>
        public Task<HandlerResponse> ExecuteAsync(CommandHandlerRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }            

            ServicesContainer servicesContainer = request.Configuration.Services;
            ICommandHandlerSelector handlerSelector = servicesContainer.GetHandlerSelector();
            CommandHandlerDescriptor descriptor = handlerSelector.SelectHandler(request);

            ICommandHandler commandHandler = descriptor.CreateHandler(request);

            if (commandHandler == null)
            {
                throw CreateHandlerNotFoundException(descriptor);
            }

            request.RegisterForDispose(commandHandler, true);
            CommandHandlerContext context = new CommandHandlerContext(request, descriptor);
            context.Handler = commandHandler;
            commandHandler.CommandContext = context;

            CommandFilterGrouping commandFilterGrouping = descriptor.GetFilterGrouping();
            
            ICommandHandlerResult result = new CommandHandlerFilterResult(context, servicesContainer, commandFilterGrouping.CommandHandlerFilters);
            
            if (descriptor.RetryPolicy != RetryPolicy.NoRetry)
            {
                result = new RetryHandlerResult(descriptor.RetryPolicy, result);
            }

            if (commandFilterGrouping.ExceptionFilters.Length > 0)
            {
                IExceptionLogger exceptionLogger = ExceptionServices.GetLogger(servicesContainer);
                IExceptionHandler exceptionHandler = ExceptionServices.GetHandler(servicesContainer);
                result = new ExceptionFilterResult(context, commandFilterGrouping.ExceptionFilters, exceptionLogger, exceptionHandler, result);
            }

            return result.ExecuteAsync(context.CancellationToken);
        }

        private static CommandHandlerNotFoundException CreateHandlerNotFoundException(CommandHandlerDescriptor descriptor)
        {
            Contract.Requires(descriptor != null);

            if (descriptor.ReturnType == typeof(VoidResult))
            {
                return new CommandHandlerNotFoundException(descriptor.MessageType);
            }

            return new CommandHandlerNotFoundException(descriptor.MessageType, descriptor.ReturnType);
        }
    }
}