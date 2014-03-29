namespace Waffle.Events
{
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;
    using Waffle.Internal;

    public class DefaultEventHandlerInvoker : IEventHandlerInvoker
    {
        public virtual Task InvokeHandlerAsync(EventHandlerContext context, CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            return InvokeActionAsyncCore(context, cancellationToken);
        }

        private static Task InvokeActionAsyncCore(EventHandlerContext context, CancellationToken cancellationToken)
        {
            Contract.Requires(context != null);
            Contract.Requires(context.Descriptor != null);

            EventHandlerDescriptor handlerDescriptor = context.Descriptor;
            return handlerDescriptor.ExecuteAsync(context, cancellationToken);
        }
  }
}