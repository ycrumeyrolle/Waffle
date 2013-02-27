namespace CommandProcessing.Filters
{
    using System;
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

        protected HandlerContext(HandlerContext context)
            : this()
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            this.Configuration = context.Configuration;
            this.Request = context.Request;
            this.Command = context.Command;
            this.Descriptor = context.Descriptor;
        }

        public ProcessorConfiguration Configuration { get; private set; }

        public HandlerRequest Request { get; private set; }

        public ICommand Command { get; private set; }

        public HandlerDescriptor Descriptor { get; private set; }

        public IDictionary<string, object> Items { get; private set; }
    }
}