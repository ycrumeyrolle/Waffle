namespace CommandProcessing.Tracing
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Data object used by <see cref="ITraceWriter"/> to record traces.
    /// </summary>
    public class TraceRecord
    {
        private readonly Lazy<Dictionary<object, object>> properties = new Lazy<Dictionary<object, object>>(() => new Dictionary<object, object>());

        private TraceKind traceKind;

        private TraceLevel traceLevel;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TraceRecord"/> class.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <param name="category">
        /// The category.
        /// </param>
        /// <param name="level">
        /// The level.
        /// </param>
        public TraceRecord(HandlerRequest request, string category, TraceLevel level)
        {
            this.Timestamp = DateTime.UtcNow;
            this.Request = request;
            this.RequestId = request != null ? request.Id : Guid.Empty;
            this.Category = category;
            this.Level = level;
        }
        
        /// <summary>
        /// Gets or sets the tracing category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public string Category { get; set; }

        /// <summary>
        /// Gets the <see cref="HandlerRequest"/>.
        /// </summary>
        /// <value>
        /// The command.
        /// </value>
        public ICommand Command { get; private set; }

        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets or sets the kind of trace.
        /// </summary>
        /// <value>
        /// The kind.
        /// </value>
        public TraceKind Kind
        {
            get
            {
                return this.traceKind;
            }

            set
            {
                TraceKindHelper.Validate(value, "value");
                this.traceKind = value;
            }
        }

        /// <summary>
        /// Gets or sets the tracing level.
        /// </summary>
        /// <value>
        /// The level.
        /// </value>
        public TraceLevel Level
        {
            get
            {
                return this.traceLevel;
            }

            set
            {
                TraceLevelHelper.Validate(value, "value");
                this.traceLevel = value;
            }
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the logical operation name being performed.
        /// </summary>
        /// <value>
        /// The operation.
        /// </value>
        public string Operation { get; set; }

        /// <summary>
        /// Gets or sets the logical name of the object performing the operation.
        /// </summary>
        /// <value>
        /// The operator.
        /// </value>
        public string Operator { get; set; }

        /// <summary>
        /// Optional user-defined property bag.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        public Dictionary<object, object> Properties
        {
            get
            {
                return this.properties.Value;
            }
        }

        /// <summary>
        /// Gets the <see cref="HandlerRequest"/>.
        /// </summary>
        /// <value>
        /// The request.
        /// </value>
        public HandlerRequest Request { get; private set; }

        /// <summary>
        /// Gets the ID  from the <see cref="Request"/>.
        /// </summary>
        /// <value>
        /// The request id.
        /// </value>
        public Guid RequestId { get; private set; }

        /// <summary>
        /// Gets the <see cref="System.DateTime"/> of this trace (via <see cref="System.DateTime.UtcNow"/>).
        /// </summary>
        /// <value>
        /// The timestamp.
        /// </value>
        public DateTime Timestamp { get; private set; }
  
        /// <summary>
        /// Gets the elapsed time of the record.
        /// </summary>
        /// <value>
        /// The elapsed time.
        /// </value>
        public TimeSpan Elapsed { get; set; }
    }
}