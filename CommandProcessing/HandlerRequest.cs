namespace CommandProcessing
{
    using System;
    using System.Collections.Generic;
    using CommandProcessing.Dependencies;
    using CommandProcessing.Internal;
    using CommandProcessing.Validation;

    /// <summary>
    /// Represents a request for an handler.    
    /// The <see cref="HandlerRequest"/> is responsible to encapsulate 
    /// all informations around a call to an handler.
    /// </summary>
    public sealed class HandlerRequest : IDisposable
    {
        // Guid.NewGuid() seems to be costly. Call it only when required.
        private readonly Lazy<Guid> id = new Lazy<Guid>(Guid.NewGuid);
        private readonly IList<IDisposable> disposableResources = new List<IDisposable>();

        private static readonly Type VoidType = typeof(VoidResult);

        private IDependencyScope dependencyScope;

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerRequest"/> class. 
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="command">The command.</param>
        public HandlerRequest(ProcessorConfiguration configuration, ICommand command)
            : this(configuration, command, VoidType, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerRequest"/> class. 
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="command">The command.</param>
        /// <param name="parentRequest">The parent request. </param>
        public HandlerRequest(ProcessorConfiguration configuration, ICommand command, HandlerRequest parentRequest)
            : this(configuration, command, VoidType, parentRequest)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerRequest"/> class. 
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="command">The command.</param>
        /// <param name="resultType">The result type.</param>
        public HandlerRequest(ProcessorConfiguration configuration, ICommand command, Type resultType)
            : this(configuration, command, resultType, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerRequest"/> class. 
        /// The request will be a child request of the <paramref name="parentRequest"/>.
        /// </summary> 
        /// <param name="configuration">The configuration.</param>
        /// <param name="command">The command.</param>
        /// <param name="resultType">The result type.</param>
        /// <param name="parentRequest">The parent request. </param>
        public HandlerRequest(ProcessorConfiguration configuration, ICommand command, Type resultType, HandlerRequest parentRequest)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            if (command == null)
            {
                throw Error.ArgumentNull("command");
            }

            if (resultType == null)
            {
                throw Error.ArgumentNull("resultType");
            }

            this.Configuration = configuration;
            this.Command = command;
            this.CommandType = command.GetType();
            this.ResultType = resultType;

            this.ParentRequest = parentRequest;
            if (parentRequest != null)
            {
                this.Processor = parentRequest.Processor;
            }
        }

        /// <summary>
        /// Gets the parent request.
        /// </summary>
        /// <value>The parent value.</value>
        public HandlerRequest ParentRequest { get; private set; }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public ProcessorConfiguration Configuration { get; private set; }

        /// <summary>
        /// Gets the current command.
        /// </summary>
        /// <value>The current command.</value>
        public ICommand Command { get; private set; }

        /// <summary>
        /// Gets the command type.
        /// </summary>
        /// <value>The command <see cref="System.Type"/>.</value>
        public Type CommandType { get; private set; }

        /// <summary>
        /// Gets the result type.
        /// </summary>
        /// <value>The result <see cref="System.Type"/>.</value>
        public Type ResultType { get; private set; }

        /// <summary>
        /// Gets the handler identifier.
        /// </summary>
        /// <value>The handler identifier.</value>
        public Guid Id
        {
            get
            {
                return this.id.Value;
            }
        }

        /// <summary>
        /// Gets or sets the processor in charge of the request.
        /// </summary>
        public ICommandProcessor Processor { get; set; }

        /// <summary>
        /// Provides a <see cref="IDependencyScope"/> for the request.
        /// </summary>
        /// <returns>A <see cref="IDependencyScope"/>.</returns>
        public IDependencyScope GetDependencyScope()
        {
            return this.GetDependencyScope(true);
        }

        /// <summary>
        /// Provides a <see cref="IDependencyScope"/> for the request.
        /// </summary>
        /// <param name="useDeepestRequest">Indicate whether to use the deepest request to locate the <see cref="IDependencyScope"/>.</param>
        /// <returns>A <see cref="IDependencyScope"/>.</returns>
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

        /// <summary>
        /// Releases the unmanaged resources that are used by the object and releases the managed resources.
        /// </summary>
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

        /// <summary>
        /// Registers a resource to be disposed at the end of the request.
        /// </summary>
        /// <param name="resource">The resource to dispose.</param>
        public void RegisterForDispose(IDisposable resource)
        {
            if (resource != null)
            {
                this.disposableResources.Add(resource);
            }
        }
    }
}