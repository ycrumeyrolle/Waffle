﻿namespace Waffle
{
    using System;
    using System.Collections.Generic;
    using Waffle.Dependencies;
    using Waffle.Internal;

    /// <summary>
    /// Represents a request for an handler.    
    /// The <see cref="HandlerRequest"/> is responsible to encapsulate 
    /// all informations around a call to an handler.
    /// </summary>
    public class HandlerRequest : IDisposable
    {
        // Guid.NewGuid() seems to be costly. Call it only when required.
        private readonly Lazy<Guid> id = new Lazy<Guid>(Guid.NewGuid);
        private readonly IList<IDisposable> disposableResources = new List<IDisposable>();
        
        private IDependencyScope dependencyScope;

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerRequest"/> class. 
        /// The request will be a child request of the <paramref name="parentRequest"/>.
        /// </summary> 
        /// <param name="configuration">The configuration.</param>
        /// <param name="parentRequest">The parent request. </param>
        public HandlerRequest(ProcessorConfiguration configuration, HandlerRequest parentRequest)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            this.Configuration = configuration;

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
        /// <value>The processor in charge of the request.</value>
        public IMessageProcessor Processor { get; set; }

        /// <summary>
        /// Gets the message type.
        /// </summary>
        /// <value>The message <see cref="System.Type"/>.</value>
        public Type MessageType { get; protected set; }

        /// <summary>
        /// Provides a <see cref="IDependencyScope"/> for the request.
        /// </summary>
        /// <returns>A <see cref="IDependencyScope"/>.</returns>
        public IDependencyScope GetDependencyScope()
        {
            return this.GetDependencyScope(useDeepestRequest: true);
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
                IDependencyScope scope = this.Configuration.DependencyResolver.BeginScope();
                request.RegisterForDispose(scope);
                request.dependencyScope = scope;
            }

            return request.dependencyScope;
        }

        /// <summary>
        /// Releases the unmanaged resources that are used by the object and releases the managed resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources that are used by the object and releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources. </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (IDisposable item in this.disposableResources)
                {
                    item.Dispose();
                }
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
        /// The resource will be disposed when the <see cref="HandlerRequest"/> will be disposed.
        /// </summary>
        /// <param name="resource">The resource to dispose.</param>
        public void RegisterForDispose(object resource)
        {
            this.RegisterForDispose(resource, false);
        }

        /// <summary>
        /// Registers a resource to be disposed at the end of the request.
        /// </summary>
        /// <param name="resource">The resource to dispose.</param> 
        /// <param name="useDeepestRequest">Indicate whether to use the deepest request to register the disposing.</param>
        public void RegisterForDispose(object resource, bool useDeepestRequest)
        {
            IDisposable disposableResource = resource as IDisposable;
            if (disposableResource != null)
            {
                HandlerRequest request = this.GetRootRequest(useDeepestRequest);
                request.disposableResources.Add(disposableResource);
            }
        }
    }
}