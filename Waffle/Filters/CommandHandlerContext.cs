namespace Waffle.Filters
{
    using System.Collections.Generic;
    using System.Security.Principal;
    using System.Threading;
    using Waffle.Commands;
    using Waffle.Internal;
    using Waffle.Validation;

    /// <summary>
    /// Contains information for the executing handler.
    /// </summary>
    public class CommandHandlerContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandlerContext"/> class. 
        /// </summary>
        public CommandHandlerContext()
        {
            this.Items = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandlerContext"/> class. 
        /// </summary>
        /// <param name="request">The handler request.</param>
        /// <param name="descriptor">The handler descriptor.</param>
        public CommandHandlerContext(CommandHandlerRequest request, CommandHandlerDescriptor descriptor)
            : this()
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (request.Configuration == null)
            {
                throw Error.Argument("request");
            }

            this.Configuration = request.Configuration;
            this.Request = request;
            this.Command = request.Command;
            this.Descriptor = descriptor;
            this.User = this.Configuration.Services.GetPrincipalProvider().Principal;
            this.CancellationToken = new CancellationToken();
        }

        /// <summary>
        /// Gets or sets the result returned by the handler method.
        /// </summary>
        /// <returns>
        /// The result returned by the handler method.
        /// </returns>
        /// <value>
        /// The result.
        /// </value>
        public HandlerResponse Response { get; set; }

        /// <summary>
        /// Gets the processor configuration.
        /// </summary>
        /// <value>The processor configuration.</value>
        public ProcessorConfiguration Configuration { get; private set; }

        /// <summary>
        /// Gets the request for the handler context.
        /// </summary>
        /// <value>The request for the handler context.</value>
        public CommandHandlerRequest Request { get; private set; }

        /// <summary>
        /// Gets the command for the handler context.
        /// </summary>
        /// <value>The command for the handler context.</value>
        public ICommand Command { get; private set; }
        
        /// <summary>
        /// Gets the descriptor for the handler context.
        /// </summary>
        /// <value>The descriptor for the handler context.</value>
        public CommandHandlerDescriptor Descriptor { get; private set; }

        /// <summary>
        /// Gets a <see cref="IDictionary{K, V}"/> of <see cref="string" />, <see cref="object" /> that can be used share data.
        /// </summary>
        /// <value>The <see cref="IDictionary{K, V}"/> of <see cref="string" />, <see cref="object" />.</value>
        public IDictionary<string, object> Items { get; private set; }
        
        /// <summary>
        /// Gets the <see cref="IPrincipal"/>.
        /// </summary>
        /// <value>The <see cref="IPrincipal"/>.</value>
        public IPrincipal User { get; internal set; }

        /// <summary>
        /// Gets the <see cref="ICommandHandler"/>.
        /// </summary>
        /// <value>The <see cref="ICommandHandler"/>.</value>
        public ICommandHandler Handler { get; internal set; }

        public CancellationToken CancellationToken { get; internal set; }

        public ModelStateDictionary ModelState
        {
            get { return this.Request.ModelState; }
        }
    }
}