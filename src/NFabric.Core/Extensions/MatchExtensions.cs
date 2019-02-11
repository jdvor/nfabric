namespace NFabric.Core.Extensions
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Extension methods for <see cref="Match"/> type.
    /// </summary>
    public static class MatchExtensions
    {
        public static string AsString(this Match m, string groupName, string fallback = null)
        {
            var s = m.Groups[groupName]?.Value;
            if (s != null)
            {
                return s;
            }

            if (fallback != null)
            {
                return fallback;
            }

            throw new ArgumentException(
                $"match group '{groupName}' not found and no fallback has been provided",
                nameof(m));
        }

        public static bool AsBool(this Match m, string groupName, bool? fallback = null)
        {
            var s = m.Groups[groupName]?.Value;
            if (s == null && fallback.HasValue)
            {
                return fallback.Value;
            }

            return s.AsBool(fallback);
        }

        public static int AsInt(this Match m, string groupName, int? fallback = null)
        {
            var s = m.Groups[groupName]?.Value;
            if (s == null && fallback.HasValue)
            {
                return fallback.Value;
            }

            return s.AsInt(fallback);
        }

        public static uint AsUInt(this Match m, string groupName, uint? fallback = null)
        {
            var s = m.Groups[groupName]?.Value;
            if (s == null && fallback.HasValue)
            {
                return fallback.Value;
            }

            return s.AsUInt(fallback);
        }

        public static long AsLong(this Match m, string groupName, long? fallback = null)
        {
            var s = m.Groups[groupName]?.Value;
            if (s == null && fallback.HasValue)
            {
                return fallback.Value;
            }

            return s.AsLong(fallback);
        }

        public static double AsDouble(this Match m, string groupName, double? fallback = null)
        {
            var s = m.Groups[groupName]?.Value;
            if (s == null && fallback.HasValue)
            {
                return fallback.Value;
            }

            return s.AsDouble(fallback);
        }

        public static DateTimeOffset AsDate(this Match m, string groupName, DateTimeOffset? fallback = null)
        {
            var s = m.Groups[groupName]?.Value;
            if (s == null && fallback.HasValue)
            {
                return fallback.Value;
            }

            return s.AsDate(fallback);
        }

        public static TimeSpan AsTimeSpan(this Match m, string groupName, TimeSpan? fallback = null)
        {
            var s = m.Groups[groupName]?.Value;
            if (s == null && fallback.HasValue)
            {
                return fallback.Value;
            }

            return s.AsTimeSpan(fallback);
        }

        public static Guid AsGuid(this Match m, string groupName)
        {
            var s = m.Groups[groupName]?.Value;
            return s.AsGuid();
        }

        public static T AsEnum<T>(this Match m, string groupName)
            where T : struct, IComparable, IConvertible, IFormattable
        {
            var s = m.Groups[groupName]?.Value;
            return s.AsEnum<T>();
        }
    }
}
