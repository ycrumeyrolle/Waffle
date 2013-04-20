namespace CommandProcessing.Descriptions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using CommandProcessing.Dispatcher;
    using CommandProcessing.Filters;
    using CommandProcessing.Internal;

    /// <summary>
    /// Explores the commands available in the system.
    /// </summary>
    public class DefaultCommandExplorer : ICommandExplorer
    {
        private readonly Lazy<Collection<CommandDescription>> descriptions;

        private readonly ProcessorConfiguration config;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCommandExplorer"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public DefaultCommandExplorer(ProcessorConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            this.config = configuration;
            this.descriptions = new Lazy<Collection<CommandDescription>>(this.InitializeDescriptions);
        }

        /// <summary>
        /// Gets the descriptions. The descriptions are initialized on the first access.
        /// </summary>
        /// <value>
        /// The descriptions.
        /// </value>
        public ICollection<CommandDescription> Descriptions
        {
            get
            {
                return this.descriptions.Value;
            }
        }

        private Collection<CommandDescription> InitializeDescriptions()
        {
            IHandlerDescriptorProvider descriptorProvider = this.config.Services.GetHandlerDescriptorProvider();
            IDictionary<Type, HandlerDescriptor> handlerMappings = descriptorProvider.GetHandlerMapping();

            return new Collection<CommandDescription>(handlerMappings.Select(m => new CommandDescription { Name = m.Value.Name, HandlerType = m.Value.HandlerType, CommandType = m.Key }).ToList());
        }
    }
}