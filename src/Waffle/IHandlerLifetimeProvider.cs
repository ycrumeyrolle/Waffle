namespace Waffle
{
    using Waffle.Filters;

    /// <summary>
    /// Provides handler lifetime.
    /// </summary>
    public interface IHandlerLifetimeProvider
    {
        /// <summary>Gets the <see cref="HandlerLifetime"/>.</summary>
        /// <returns>An <see cref="HandlerLifetime"/>.</returns>
        HandlerLifetime HandlerLifetime { get; }
    }
}
