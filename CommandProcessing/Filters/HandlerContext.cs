namespace CommandProcessing.Filters
{
    using System.Collections.Generic;
    using CommandProcessing.Internal;

    /// <summary>
    /// Contains information for the executing handler.
    /// </summary>
    public class HandlerContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerContext"/> class. 
        /// </summary>
        public HandlerContext()
        {
            this.Items = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerContext"/> class. 
        /// </summary>
        /// <param name="request">The handler request.</param>
        /// <param name="descriptor">The handler descriptor.</param>
        public HandlerContext(HandlerRequest request, HandlerDescriptor descriptor)
            : this()
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            this.Configuration = request.Configuration;
            this.Request = request;
            this.Command = request.Command;
            this.Descriptor = descriptor;
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
        public object Result { get; set; }

        /// <summary>
        /// Gets the processor configuration.
        /// </summary>
        /// <value>The processor configuration.</value>
        public ProcessorConfiguration Configuration { get; private set; }

        /// <summary>
        /// Gets the request for the handler context.
        /// </summary>
        /// <value>The request for the handler context.</value>
        public HandlerRequest Request { get; private set; }

        /// <summary>
        /// Gets the command for the handler context.
        /// </summary>
        /// <value>The command for the handler context.</value>
        public ICommand Command { get; private set; }
        
        /// <summary>
        /// Gets the descriptor for the handler context.
        /// </summary>
        /// <value>The descriptor for the handler context.</value>
        public HandlerDescriptor Descriptor { get; private set; }

        public IDictionary<string, object> Items { get; private set; }
    }
}