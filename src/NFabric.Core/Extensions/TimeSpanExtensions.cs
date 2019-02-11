namespace NFabric.Core.Extensions
{
    using System;

    /// <summary>
    /// Extension methods for <see cref="TimeSpan"/> type.
    /// </summary>
    public static class TimeSpanExtensions
    {
        public static TimeSpan Add(
            this TimeSpan ts,
            int days = 0,
            int hours = 0,
            int minutes = 0,
            int seconds = 0,
            int millis = 0)
        {
            return ts.Add(new TimeSpan(days, hours, minutes, seconds, millis));
        }
    }
}
