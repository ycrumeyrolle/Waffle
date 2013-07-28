namespace Waffle.Events
{
    /// <summary>
    /// Default implementation of an <see cref="IEventHandlerActivator"/>.
    /// A different implementation can be registered via the <see cref="T:Waffle.Dependencies.IDependencyResolver"/>.
    /// </summary>
    public class DefaultEventHandlerActivator : HandlerActivator<IEventHandler>, IEventHandlerActivator
    {
    }
}