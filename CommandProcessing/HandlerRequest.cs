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

            // TODO : To implements
            this.ParentRequest = null;
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
            if (this.dependencyScope == null)
            {
                this.dependencyScope = this.Configuration.DependencyResolver.BeginScope();
                this.RegisterForDispose(this.dependencyScope);
            }

            return this.dependencyScope;
        }

        public void Dispose()
        {
            foreach (IDisposable item in this.disposableResources)
            {
                item.Dispose();
            }
        }

        private void RegisterForDispose(IDisposable resource)
        {
            if (resource != null)
            {
                this.disposableResources.Add(resource);
            }
        }
    }
}