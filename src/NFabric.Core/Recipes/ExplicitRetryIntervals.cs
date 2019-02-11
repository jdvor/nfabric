namespace NFabric.Core.Recipes
{
    using System;
    using System.Linq;

    /// <summary>
    /// Re-try strategy where all intervals are explicitly defined as a sequence of numbers representing milliseconds.
    /// </summary>
    public class ExplicitRetryIntervals : IRetryIntervals
    {
        public ExplicitRetryIntervals(int[] millis)
        {
            intervals = millis.Where(x => x > 0).ToArray();
        }

        private readonly int[] intervals;
        private int at;

        public TimeSpan Next()
        {
            if (at < intervals.Length)
            {
                var t = TimeSpan.FromMilliseconds(intervals[at]);
                at += 1;
                return t;
            }

            return TimeSpan.Zero;
        }

        public override string ToString()
        {
            return $"intervals {string.Join(", ", intervals)} ms";
        }
    }
}
