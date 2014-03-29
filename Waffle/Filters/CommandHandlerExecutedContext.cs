namespace Waffle.Filters
{
    using System.Runtime.ExceptionServices;
    using Waffle.Commands;
    using Waffle.Internal;

    /// <summary>
    /// Contains information for the executed handler.
    /// </summary>
    public class CommandHandlerExecutedContext
    {
        private readonly CommandHandlerContext handlerContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandlerExecutedContext"/> class.
        /// </summary>
        /// <param name="handlerContext">Then handler context.</param>
        /// <param name="exceptionInfo">The <see cref="ExceptionDispatchInfo"/>. Optionnal.</param>
        public CommandHandlerExecutedContext(CommandHandlerContext handlerContext, ExceptionDispatchInfo exceptionInfo)
        {
            if (handlerContext == null)
            {
                throw Error.ArgumentNull("handlerContext");
            }

            this.ExceptionInfo = exceptionInfo;
            this.handlerContext = handlerContext;
        }

        /// <summary>
        /// Gets or sets the handler context. 
        /// </summary>
        /// <value>The handler context.</value>
        public CommandHandlerContext HandlerContext
        {
            get
            {
                return this.handlerContext;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ExceptionDispatchInfo"/> that was raised during the execution.
        /// </summary>
        /// <value>The <see cref="ExceptionDispatchInfo"/> that was raised during the execution.</value>
        public ExceptionDispatchInfo ExceptionInfo { get; set; }

        /// <summary>
        /// Gets or sets the handler result.
        /// </summary>
        /// <value>The handler result.</value>
        public HandlerResponse Response
        {
            get
            {
                return this.handlerContext.Response;
            }

            set
            {
                this.handlerContext.Response = value;
            }
        }

        /// <summary>
        /// Gets the current <see cref="ICommand"/>.
        /// </summary>
        /// <value>
        /// The <see cref="ICommand"/>.
        /// </value>
        public ICommand Command
        {
            get
            {
                return this.handlerContext.Request.Command;
            }
        }

        /// <summary>
        /// Gets the current <see cref="HandlerRequest"/>.
        /// </summary>
        /// <value>
        /// The <see cref="HandlerRequest"/>.
        /// </value>
        public CommandHandlerRequest Request
        {
            get
            {
                return this.handlerContext.Request;
            }
        }
    }
}