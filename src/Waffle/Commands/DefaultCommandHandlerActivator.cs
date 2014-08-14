namespace Waffle.Commands
{
    /// <summary>
    /// Default implementation of an <see cref="ICommandHandlerActivator"/>.
    /// A different implementation can be registered via the <see cref="T:Waffle.Dependencies.IDependencyResolver"/>.
    /// </summary>
    public class DefaultCommandHandlerActivator : HandlerActivator<ICommandHandler>, ICommandHandlerActivator
    {
    }
}