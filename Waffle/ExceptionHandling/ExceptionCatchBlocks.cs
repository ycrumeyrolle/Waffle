namespace Waffle.ExceptionHandling
{
    using Waffle.Filters;

    /// <summary>Provides the catch blocks used within this assembly.</summary>
    public static class ExceptionCatchBlocks
    {
        private static readonly ExceptionContextCatchBlock MessageProcessorCatchBlock = new ExceptionContextCatchBlock(typeof(MessageProcessor).Name, isTopLevel: true, callsHandler: true);

        private static readonly ExceptionContextCatchBlock ExceptionFilterCatchBlock = new ExceptionContextCatchBlock(typeof(IExceptionFilter).Name, isTopLevel: false, callsHandler: true);

        /// <summary>
        /// Gets the catch block when using <see cref="ExceptionFilter"/>.
        /// </summary>
        public static ExceptionContextCatchBlock ExceptionFilter
        {
            get { return ExceptionFilterCatchBlock; }
        }

        /// <summary>
        /// Gets the catch block when using <see cref="MessageProcessor"/>.
        /// </summary>
        public static ExceptionContextCatchBlock MessageProcessor
        {
            get { return MessageProcessorCatchBlock; }
        }
    }
}
