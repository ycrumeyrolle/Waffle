namespace CommandProcessing
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using CommandProcessing.Internal;
    using CommandProcessing.Services;

    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "ServicesContainer is disposed with the configuration")]
    public sealed class HandlerSettings
    {
        private readonly ProcessorConfiguration configuration;

        private ServicesContainer services;

        public HandlerSettings(ProcessorConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            this.configuration = configuration;
        }

        public bool AbortOnInvalidCommand { get; set; }

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