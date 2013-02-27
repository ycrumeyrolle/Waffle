namespace CommandProcessing
{
    using CommandProcessing.Filters;

    // public interface ICommandHandler<in TCommand>
    // where TCommand : ICommand
    // {
    // void Handle(TCommand command);
    // }
    public interface ICommandHandler
    {
        CommandProcessor Processor { get; }

        object Handle(ICommand command);
    }

    public interface ICommandHandler<in TCommand, out TResult> : ICommandHandler
        where TCommand : ICommand
    {
        TResult Handle(TCommand command);
    }

    public interface ICommandHandler<in TCommand> : ICommandHandler<TCommand, EmptyResult>
        where TCommand : ICommand
    {
    }

    // public interface ICommandHandler<in TCommand> : ICommandHandler<TCommand, VoidResult>
    // {
    // void Handle(TCommand command);
    // }
}