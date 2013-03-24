namespace CommandProcessing.Filters
{
    public interface IHandlerConfiguration
    {
        void Initialize(HandlerSettings settings, HandlerDescriptor descriptor);
    }
}
