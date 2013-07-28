namespace Waffle.Tracing
{
    using System.Diagnostics.Contracts;
    using Waffle.Commands;
    using Waffle.Internal;
    using Waffle.Properties;
    using Waffle.Validation;

    internal class CommandValidatorTracer : ICommandValidator, IDecorator<ICommandValidator>
    {
        private const string SelectActionMethodName = "SelectHandler";

        private readonly ICommandValidator innerValidator;
        private readonly ITraceWriter traceWriter;

        public CommandValidatorTracer(ICommandValidator innerValidator, ITraceWriter traceWriter)
        {
            Contract.Assert(innerValidator != null);
            Contract.Assert(traceWriter != null);

            this.innerValidator = innerValidator;
            this.traceWriter = traceWriter;
        }

        public ICommandValidator Inner
        {
            get { return this.innerValidator; }
        }

        /// <summary>
        /// Determines whether the command is valid and adds any validation errors to the command's ValidationResults.
        /// </summary>
        /// <param name="request">The <see cref="HandlerRequest"/> to be validated.</param>
        /// <returns>true if command is valid, false otherwise.</returns>
        public bool Validate(CommandHandlerRequest request)
        {
            return this.traceWriter.TraceBeginEnd(
                request,
                TraceCategories.HandlersCategory,
                TraceLevel.Info,
                this.innerValidator.GetType().Name,
                SelectActionMethodName,
                beginTrace: tr =>
                    {
                        tr.Message = Error.Format(Resources.TraceRequestValidatedMessage, request.MessageType.FullName);
                    },
                execute: () => this.innerValidator.Validate(request),
                endTrace: null,
                errorTrace: null);
        }
    }
}