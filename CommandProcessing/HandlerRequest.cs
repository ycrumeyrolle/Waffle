namespace CommandProcessing
{
    using System;
    using System.Collections.Generic;
    using CommandProcessing.Dependencies;

    public sealed class HandlerRequest : IDisposable
    {
        private readonly Guid id = Guid.NewGuid();

        private readonly IList<IDisposable> disposableResources = new List<IDisposable>();

        private IDependencyScope dependencyScope;

        public HandlerRequest(ProcessorConfiguration configuration, ICommand command)
            : this(configuration, command, null)
        {
        }

        public HandlerRequest(ProcessorConfiguration configuration, ICommand command, HandlerRequest parentRequest)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            this.Configuration = configuration;
            this.Command = command;
            this.CommandType = command.GetType();

            this.ParentRequest = parentRequest;
        }

        public HandlerRequest ParentRequest { get; private set; }

        public ProcessorConfiguration Configuration { get; private set; }

        public ICommand Command { get; private set; }

        public Type CommandType { get; private set; }

        public Guid Id
        {
            get
            {
                return this.id;
            }
        }

        public IDependencyScope GetDependencyScope()
        {
            return this.GetDependencyScope(true);
        }

        public IDependencyScope GetDependencyScope(bool useDeepestRequest)
        {
            HandlerRequest request = this.GetRootRequest(useDeepestRequest);

            if (request.dependencyScope == null)
            {
                request.dependencyScope = this.Configuration.DependencyResolver.BeginScope();
                request.RegisterForDispose(this.dependencyScope);
            }

            return request.dependencyScope;
        }

        public void Dispose()
        {
            foreach (IDisposable item in this.disposableResources)
            {
                item.Dispose();
            }
        }

        private HandlerRequest GetRootRequest(bool useDeepestRequest)
        {
            if (!useDeepestRequest)
            {
                return this;
            }

            HandlerRequest request = this;
            while (request.ParentRequest != null)
            {
                request = request.ParentRequest;
            }

            return request;
        }

        public void RegisterForDispose(IDisposable resource)
        {
            if (resource != null)
            {
                this.disposableResources.Add(resource);
            }
        }
    }
}