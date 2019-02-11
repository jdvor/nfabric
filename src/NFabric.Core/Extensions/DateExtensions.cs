namespace NFabric.Core.Extensions
{
    using System;

    /// <summary>
    /// Extension methods for <see cref="DateTimeOffset"/>, <see cref="DateTime"/> types.
    /// </summary>
    public static class DateExtensions
    {
        public const int DaysInWeek = 7;
        public const int SecondsInMinute = 60;
        public const int SecondsInHour = 3600;
        public const int SecondsInDay = 86400;
        public const int SecondsInWeek = DaysInWeek * SecondsInDay;
        public static readonly DateTimeOffset UnixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
        public static readonly DateTimeOffset CenturyEpoch = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

        /// <summary>
        /// Returns milliseconds since 1970-01-01 00:00:00.
        /// </summary>
        [Obsolete("use DateTimeOffset.ToUnixTimeMilliseconds")]
        public static long ToMillisSinceUnixEpoch(this DateTimeOffset dt)
        {
            return Convert.ToInt64((dt - UnixEpoch).TotalMilliseconds);
        }

        /// <summary>
        /// Returns milliseconds since 2000-01-01 00:00:00.
        /// </summary>
        public static long ToMillisSinceCentury(this DateTimeOffset dt)
        {
            return Convert.ToInt64((dt - CenturyEpoch).TotalMilliseconds);
        }

        /// <summary>
        /// Returns seconds since 1970-01-01 00:00:00.
        /// </summary>
        [Obsolete("use DateTimeOffset.ToUnixTimeSeconds")]
        public static int ToSecsSinceUnixEpoch(this DateTimeOffset dt)
        {
            return Convert.ToInt32((dt - UnixEpoch).TotalSeconds);
        }

        /// <summary>
        /// Returns seconds since 2000-01-01 00:00:00.
        /// </summary>
        public static int ToSecsSinceCentury(this DateTimeOffset dt)
        {
            return Convert.ToInt32((dt - CenturyEpoch).TotalSeconds);
        }
    }
}
