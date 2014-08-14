namespace Waffle.Commands
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides informations about the command handler method of an <see cref="ICommand"/> type.
    /// </summary>
    public class CommandHandlerDescriptor<TCommand> : CommandHandlerDescriptor where TCommand : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandlerDescriptor"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="ProcessorConfiguration"/>.</param>
        /// <param name="commandType">The type of the command.</param>
        /// <param name="func">The <see cref="Func{T}"/> representing the handler.</param>
        public CommandHandlerDescriptor(ProcessorConfiguration configuration, Type commandType, Func<TCommand, Task> func)
            : base(configuration, commandType, typeof(FuncCommandHandler<TCommand>), func.GetMethodInfo())
        {
        }
    }
}