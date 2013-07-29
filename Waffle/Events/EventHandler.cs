namespace Waffle.Events
{
    using System.Diagnostics.CodeAnalysis;
    using Waffle.Internal;

    /// <summary>
    /// Represents a base implementation of the the event handler. 
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "This interface is similar to the EventHandler from .Net.")]
    public abstract class EventHandler<TEvent> : IEventHandler<TEvent> where TEvent : IEvent
    {
        /// <summary>
        /// Handle the event.
        /// </summary>
        /// <param name="event">The <see cref="IEvent"/> to handle.</param>
        /// <param name="context">The <see cref="EventHandlerContext"/>.</param>
        public abstract void Handle(TEvent @event, EventHandlerContext context);

        /// <summary>
        /// Handle the event.
        /// </summary>
        /// <param name="event">The <see cref="IEvent"/> to handle.</param>
        /// <param name="context">The <see cref="EventHandlerContext"/>.</param>
        public void Handle(IEvent @event, EventHandlerContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            context.Descriptor.HandleMethod(this, @event, context);
        }
    }
}
