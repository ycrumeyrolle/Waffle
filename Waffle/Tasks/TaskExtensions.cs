namespace Waffle.Tasks
{
    using System.Threading.Tasks;

    /// <summary>
    /// Provides extensions methods for the <see cref="Task"/> type.
    /// </summary>
    public static class TaskExtensions
    { 
        /// <summary>
        /// Cast Task to Task of object.
        /// </summary>
        internal static async Task<object> CastToObject(this Task task)
        {
            await task;
            return null;
        }

        /// <summary>
        /// Cast Task of T to Task of object.
        /// </summary>
        internal static async Task<object> CastToObject<T>(this Task<T> task)
        {
            return (object)await task;
        }
    }
}
