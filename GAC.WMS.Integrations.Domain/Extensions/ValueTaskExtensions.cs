using System.Runtime.CompilerServices;

namespace GAC.WMS.Integrations.Domain.Extensions
{
    public static class ValueTaskExtensions
    {
        /// <summary>
        /// Safely awaits a nullable ValueTask
        /// </summary>
        public static ValueTask AsValueTask(this ValueTask? task)
        {
            return task ?? default;
        }

        /// <summary>
        /// Safely awaits a nullable ValueTask<T>
        /// </summary>
        public static ValueTask<T?> AsValueTask<T>(this ValueTask<T>? task)
        {
            return task ?? new ValueTask<T?>(default(T));
        }

        ///// <summary>
        ///// Gets an awaiter for a nullable ValueTask
        ///// </summary>
        //public static TaskAwaiter GetAwaiter(this ValueTask? task)
        //{
        //    return (task ?? default).GetAwaiter();
        //}

        ///// <summary>
        ///// Gets an awaiter for a nullable ValueTask<T>
        ///// </summary>
        //public static TaskAwaiter<T?> GetAwaiter<T>(this ValueTask<T>? task)
        //{
        //    return (task ?? new ValueTask<T?>(default(T))).GetAwaiter();
        //}
    }
}
