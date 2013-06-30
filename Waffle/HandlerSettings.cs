namespace Waffle
{
    using System.Diagnostics.CodeAnalysis;
    using Waffle.Internal;
    using Waffle.Services;

    /// <summary>
    /// Contains settings for an handler. 
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "ServicesContainer is disposed with the configuration")]
    public sealed class HandlerSettings
    {
        private readonly ProcessorConfiguration configuration;

        private ServicesContainer services;

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerSettings"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public HandlerSettings(ProcessorConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            this.configuration = configuration;
        }

        /// <summary>
        /// Gets or sets whether the command should be aborted on invalid command.
        /// </summary>
        /// <value><c>true</c> if the command should be aborted on invalid command ; false otherwise.</value>
        public bool AbortOnInvalidCommand { get; set; }

        /// <summary>
        /// Gets the collection of service instances for the handler.
        /// </summary>
        /// <value>The collection of service instances for the handler.</value>
        public ServicesContainer Services
        {
            get
            {
                if (this.services == null)
                {
                    this.services = new HandlerServices(this.configuration.Services);
                }

                return this.services;
            }
        }

        internal bool IsServiceCollectionInitialized
        {
            get
            {
                return this.services != null;
            }
        }
    }
}