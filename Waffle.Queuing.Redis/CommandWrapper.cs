namespace Waffle.Queuing.Redis
{
    using System;
    using Waffle.Commands;

    /// <summary>
    /// Represents a wrapper to store a command in MongoDB.
    /// </summary>
    public class CommandWrapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventWrapper"/> class.
        /// </summary>
        /// <param name="payload">The payload to wrap.</param>
        public CommandWrapper(ICommand payload)
        {
            if (payload == null)
            {
                throw new ArgumentNullException("payload");
            }

            this.Id = Guid.NewGuid();
            this.CreationDate = DateTime.Now;
            this.Payload = payload;
        }

        /// <summary>
        /// Gets or sets the event id.
        /// </summary>
        public Guid Id { get; set; }
                
        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the payload.
        /// </summary>
        public ICommand Payload { get; set; }
    }
}
