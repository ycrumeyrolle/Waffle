namespace CommandProcessing.Filters
{
    using System.Collections.Generic;

    public class HandlerContext
    {
        public HandlerContext()
        {
            this.Items = new Dictionary<string, object>();
        }

        public HandlerContext(HandlerRequest request, HandlerDescriptor descriptor)
            : this()
        {
            this.Configuration = request.Configuration;
            this.Request = request;
            this.Command = request.Command;
            this.Descriptor = descriptor;
        }

        /// <summary>
        /// Gets or sets the result returned by the action method.
        /// </summary>
        /// <returns>
        /// The result returned by the action method.
        /// </returns>
        /// <value>
        /// The result.
        /// </value>
        public object Result { get; set; }

        public ProcessorConfiguration Configuration { get; private set; }

        public HandlerRequest Request { get; private set; }

        public ICommand Command { get; private set; }

        public HandlerDescriptor Descriptor { get; private set; }

        public IDictionary<string, object> Items { get; private set; }
    }
}