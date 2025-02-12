using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace Sentry.Internal.Extensions
{
    internal static class MiscExtensions
    {
        public static TOut Pipe<TIn, TOut>(this TIn input, Func<TIn, TOut> pipe) => pipe(input);

        public static T? NullIfDefault<T>(this T value) where T : struct =>
            !EqualityComparer<T>.Default.Equals(value, default)
                ? value
                : null;

        public static string ToHexString(this long l) =>
            "0x" + l.ToString("x", CultureInfo.InvariantCulture);

        private static readonly TimeSpan MaxTimeout = TimeSpan.FromMilliseconds(int.MaxValue);

        public static void CancelAfterSafe(this CancellationTokenSource cts, TimeSpan timeout)
        {
            if (timeout == TimeSpan.Zero)
            {
                // CancelAfter(TimeSpan.Zero) may not cancel immediately, but Cancel always will.
                cts.Cancel();
            }
            else if (timeout > MaxTimeout)
            {
                // Timeout milliseconds can't be larger than int.MaxValue
                // Treat such values (i.e. TimeSpan.MaxValue) as an infinite timeout (-1 ms).
                cts.CancelAfter(Timeout.InfiniteTimeSpan);
            }
            else
            {
                // All other timeout values
                cts.CancelAfter(timeout);
            }
        }

        /// <summary>
        /// Determines whether an object is <c>null</c>.
        /// </summary>
        /// <remarks>
        /// This method exists so that we can test for null in situations where a method might be called from
        /// code that ignores nullability warnings.
        /// (It prevents us having to have two different resharper ignore comments depending on target framework.)
        /// </remarks>
        public static bool IsNull(this object? o) => o is null;
    }
}
