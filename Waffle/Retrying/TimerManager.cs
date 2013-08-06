namespace Waffle.Retrying
{
    using System.Collections.Generic;
    using System.Threading;

    internal static class TimerManager
    {
        private static readonly Dictionary<Timer, object> RootedTimers = new Dictionary<Timer, object>();

        public static void Add(Timer timer)
        {
            lock (TimerManager.RootedTimers)
            {
                TimerManager.RootedTimers.Add(timer, null);
            }
        }

        public static void Remove(Timer timer)
        {
            lock (TimerManager.RootedTimers)
            {
                TimerManager.RootedTimers.Remove(timer);
            }
        }
    }
}
