using System.Diagnostics;

namespace Demo.Tools.Common.Extensions
{
    public static class StopwatchExtensions
    {
        /// <summary>
        /// Returns elapsed time in nanoseconds (ns)
        /// </summary>
        /// <param name="stopwatch"></param>
        /// <returns></returns>
        public static double ElapsedNanoseconds(this Stopwatch stopwatch) => stopwatch.ElapsedTicks / (double)Stopwatch.Frequency * 1_000_000_000;

        /// <summary>
        /// Returns elapsed time in microseconds (μs)
        /// </summary>
        /// <param name="stopwatch"></param>
        /// <returns></returns>
        public static double ElapsedMicroseconds(this Stopwatch stopwatch) => stopwatch.ElapsedTicks / (double)Stopwatch.Frequency * 1_000_000;

        /// <summary>
        /// Returns elapsed time in miliseconds (ms)
        /// </summary>
        /// <param name="stopwatch"></param>
        /// <returns></returns>
        public static double ElapsedMilliseconds(this Stopwatch stopwatch) => stopwatch.ElapsedTicks / (double)Stopwatch.Frequency * 1_000;
    }
}