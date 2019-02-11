namespace NFabric.Core.Extensions
{
    using NFabric.Core.Serialization;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using static System.Text.RegularExpressions.RegexOptions;

    /// <summary>
    /// Extension methods for <see cref="string"/> type.
    /// </summary>
    public static class StringExtensions
    {
        private static readonly Regex TimeSpanRgx = new Regex(
            @"^(?<num>\d+)\s*(?<unit>ms|millis|millisecond|milliseconds|s|sec|secs|second|seconds" +
            @"|m|min|mins|minute|minutes|h|hrs|hour|hours|d|day|days)$",
            Compiled | IgnoreCase | CultureInvariant);

        private static readonly string[] TrueVals = { "yes", "on", "1" };

        private static readonly string[] FalseVals = { "no", "off", "0" };

        private static readonly Regex WhiteSpaceRx = new Regex(@"\s+", Compiled);

        private static readonly Regex NonSafeRx = new Regex(@"[^\w]+", Compiled);

        public static bool AsBool(this string s, bool? fallback = null)
        {
            var trimmed = s?.Trim() ?? string.Empty;
            if (trimmed.Length < 4)
            {
                if (TrueVals.Contains(s, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (FalseVals.Contains(s, StringComparer.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            if (bool.TryParse(trimmed, out var v))
            {
                return v;
            }

            if (fallback.HasValue)
            {
                return fallback.Value;
            }

            throw new InvalidCastException("string could not be parsed as bool");
        }

        public static int AsInt(this string s, int? fallback = null)
        {
            if (int.TryParse(s, out var v))
            {
                return v;
            }

            if (fallback.HasValue)
            {
                return fallback.Value;
            }

            throw new InvalidCastException("string could not be parsed as int");
        }

        public static uint AsUInt(this string s, uint? fallback = null)
        {
            if (uint.TryParse(s, out var v))
            {
                return v;
            }

            if (fallback.HasValue)
            {
                return fallback.Value;
            }

            throw new InvalidCastException("string could not be parsed as uint");
        }

        public static long AsLong(this string s, long? fallback = null)
        {
            if (long.TryParse(s, out var v))
            {
                return v;
            }

            if (fallback.HasValue)
            {
                return fallback.Value;
            }

            throw new InvalidCastException("string could not be parsed as long");
        }

        public static double AsDouble(this string s, double? fallback = null)
        {
            if (double.TryParse(s, out var v))
            {
                return v;
            }

            if (fallback.HasValue)
            {
                return fallback.Value;
            }

            throw new InvalidCastException("string could not be parsed as double");
        }

        public static byte AsByte(this string s, byte? fallback = null)
        {
            byte v;
            if (s.StartsWith("0x") && byte.TryParse(s.Substring(2), NumberStyles.HexNumber, null, out v))
            {
                return v;
            }

            if (byte.TryParse(s, out v))
            {
                return v;
            }

            if (fallback.HasValue)
            {
                return fallback.Value;
            }

            throw new InvalidCastException("string could not be parsed as byte");
        }

        public static DateTimeOffset AsDate(this string s, DateTimeOffset? fallback = null)
        {
            var trimmed = s?.Trim() ?? string.Empty;
            if (DateTimeOffset.TryParse(trimmed, out var v))
            {
                return v;
            }

            if (fallback.HasValue)
            {
                return fallback.Value;
            }

            throw new InvalidCastException("string could not be parsed as DateTimeOffset");
        }

        public static DateTime AsLegacyDate(this string s, DateTime? fallback = null)
        {
            var trimmed = s?.Trim() ?? string.Empty;
            if (DateTime.TryParse(trimmed, out var v))
            {
                return v;
            }

            if (fallback.HasValue)
            {
                return fallback.Value;
            }

            throw new InvalidCastException("string could not be parsed as DateTime");
        }

        public static TimeSpan AsTimeSpan(this string s, TimeSpan? fallback = null)
        {
            var trimmed = s?.Trim() ?? string.Empty;
            if (TimeSpan.TryParse(trimmed, out var v))
            {
                return v;
            }

            if (TryParse(trimmed, out v))
            {
                return v;
            }

            if (fallback.HasValue)
            {
                return fallback.Value;
            }

            throw new InvalidCastException("string could not be parsed as TimeSpan");
        }

        public static IPAddress AsIPAddress(this string s, IPAddress fallback = null)
        {
            if (IPAddress.TryParse(s, out var ip))
            {
                return ip;
            }

            if (fallback != null)
            {
                return fallback;
            }

            throw new InvalidCastException("string could not be parsed as IPAddress");
        }

        public static Guid AsGuid(this string s)
        {
            var trimmed = s?.Trim() ?? string.Empty;
            if (Guid.TryParse(trimmed, out var v))
            {
                return v;
            }

            throw new InvalidCastException("string could not be parsed as Guid");
        }

        public static T AsEnum<T>(this string s)
            where T : struct, IComparable, IConvertible, IFormattable
        {
            var trimmed = s?.Trim() ?? string.Empty;
            if (Enum.TryParse(trimmed, out T v))
            {
                return v;
            }

            throw new InvalidCastException($"string could not be parsed as enum {typeof(T).Name}");
        }

        public static FileInfo AsFile(this string s, bool mustExist = false, string fallback = null)
        {
            var v = new FileInfo(s);
            if (mustExist)
            {
                if (v.Exists)
                {
                    return v;
                }

                if (!string.IsNullOrEmpty(fallback))
                {
                    v = new FileInfo(fallback);
                    if (v.Exists)
                    {
                        return v;
                    }
                }

                throw new InvalidCastException("string represents file path that does not exist");
            }

            return v;
        }

        public static DirectoryInfo AsDir(this string s, bool mustExist = false, string fallback = null)
        {
            var v = new DirectoryInfo(s);
            if (mustExist)
            {
                if (v.Exists)
                {
                    return v;
                }

                if (!string.IsNullOrEmpty(fallback))
                {
                    v = new DirectoryInfo(fallback);
                    if (v.Exists)
                    {
                        return v;
                    }
                }

                throw new InvalidCastException("string represents directory path that does not exist");
            }

            return v;
        }

        public static byte[] AsBytesFromHex(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return Array.Empty<byte>();
            }

            if (s.Length % 2 == 1)
            {
                throw new ArgumentException("The hex string cannot have an odd number of characters.", nameof(s));
            }

            byte[] arr = new byte[s.Length >> 1];
            if (char.IsUpper(s[0])) // assumption: if first char is uppercase, all the rest must be also
            {
                for (int i = 0; i < s.Length >> 1; ++i)
                {
                    arr[i] = (byte)((GetHexFromUpper(s[i << 1]) << 4) + GetHexFromUpper(s[(i << 1) + 1]));
                }
            }
            else
            {
                for (int i = 0; i < s.Length >> 1; ++i)
                {
                    arr[i] = (byte)((GetHexFromLower(s[i << 1]) << 4) + GetHexFromLower(s[(i << 1) + 1]));
                }
            }

            return arr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetHexFromUpper(char hex)
        {
            int val = hex;
            return val - (val < 58 ? 48 : 55);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetHexFromLower(char hex)
        {
            int val = hex;
            return val - (val < 58 ? 48 : 87);
        }

        public static byte[] AsBytesFromBase64(this string s, Base64Options options = Base64Options.Default)
        {
            if (string.IsNullOrEmpty(s))
            {
                return Array.Empty<byte>();
            }

            if (options == Base64Options.Default)
            {
                return Convert.FromBase64String(s);
            }

            if (options.HasFlag(Base64Options.NoPadding) || options.HasFlag(Base64Options.UrlSafe))
            {
                switch (s.Length % 4)
                {
                    case 2:
                        s += "==";
                        break;

                    case 3:
                        s += "=";
                        break;
                }
            }

            if (options.HasFlag(Base64Options.UrlSafe))
            {
                s = s.Replace('_', '/').Replace('-', '+');
            }

            return Convert.FromBase64String(s);
        }

        public static byte[] AsBytesFromHumanBase(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return Array.Empty<byte>();
            }

            return HumanBaseEncoding.Instance.Value.Decode(s);
        }

        public static object As(this string s, Type type, bool @throw = false)
        {
            if (type == typeof(string))
            {
                return s;
            }

            try
            {
                var convType = Nullable.GetUnderlyingType(type) ?? type;
                var ti = convType.GetTypeInfo();
                if (ti.IsValueType)
                {
                    if (convType == typeof(bool))
                    {
                        return AsBool(s);
                    }

                    if (convType == typeof(DateTimeOffset))
                    {
                        return AsDate(s);
                    }

                    if (convType == typeof(DateTime))
                    {
                        return AsLegacyDate(s);
                    }

                    if (convType == typeof(TimeSpan))
                    {
                        return AsTimeSpan(s);
                    }

                    if (convType == typeof(Guid))
                    {
                        return AsGuid(s);
                    }

                    if (ti.IsEnum)
                    {
                        var trimmed = s?.Trim() ?? string.Empty;
                        return Enum.Parse(convType, trimmed, true);
                    }

                    if (convType == typeof(byte))
                    {
                        return AsByte(s);
                    }
                }

                if (ti.IsClass)
                {
                    if (convType == typeof(FileInfo))
                    {
                        return AsFile(s);
                    }

                    if (convType == typeof(DirectoryInfo))
                    {
                        return AsDir(s);
                    }
                }

                return Convert.ChangeType(s, convType);
            }
            catch
            {
                if (@throw)
                {
                    throw;
                }

                return null;
            }
        }

        public static string Interleave(this string s, string insert, int everyNth, bool allowTrailing = false)
        {
            if (string.IsNullOrEmpty(s)
                || string.IsNullOrEmpty(insert)
                || everyNth < 1
                || s.Length <= everyNth)
            {
                return s;
            }

            var sb = new StringBuilder();
            for (var pos = 0; pos < s.Length; pos += everyNth)
            {
                var everyNthOrRem = s.Length - pos > everyNth
                    ? everyNth
                    : s.Length - pos;
                sb.Append(s.Substring(pos, everyNthOrRem));
                if (pos + everyNth >= s.Length)
                { // last iteration
                    if (s.Length % everyNth == 0 && allowTrailing)
                    { // has trailing insertion and it is allowed
                        sb.Append(insert);
                    }
                }
                else
                {
                    sb.Append(insert);
                }
            }

            return sb.ToString();
        }

        public static string Repeat(this string s, int count, string sep = null)
        {
            var placeSep = !string.IsNullOrEmpty(sep);
            var sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                if (placeSep && i > 0)
                {
                    sb.Append(sep);
                }

                sb.Append(s);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Joins together results of .ToString() of each item within <paramref name="items"/>; separates them by <paramref name="sep"/>;
        /// prepends by <paramref name="prefix"/> and appends by <paramref name="sufix"/>. In all cases <c>null</c> is interpreted as empty string.
        /// </summary>
        public static string MakeString<T>(this ICollection<T> items, string sep = null, string prefix = null, string sufix = null)
        {
            var sb = string.IsNullOrEmpty(prefix) ? new StringBuilder() : new StringBuilder(prefix);
            var placeSep = !string.IsNullOrEmpty(sep);
            var first = true;
            foreach (var item in items)
            {
                if (first)
                {
                    first = false;
                }
                else if (placeSep)
                {
                    sb.Append(sep);
                }

                sb.Append(item);
            }

            if (!string.IsNullOrEmpty(prefix))
            {
                sb.Append(sufix);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Tries to find first match from <paramref name="any"/> within <paramref name="s"/>.
        /// If succeeded it returns <c>true</c> along with which token and position it has found.
        /// </summary>
        /// <param name="any">Array of strings to attempt.</param>
        /// <param name="fromIndex">Limit searching withing <paramref name="s"/> from this position.</param>
        /// <param name="found">Output parameter with tuple of token and position when any is found.</param>
        /// <returns></returns>
        public static bool TryIndexOfAny(this string s, string[] any, int fromIndex, out Tuple<string, int> found)
        {
            if (string.IsNullOrEmpty(s) || any == null || any.Length == 0 || fromIndex < 0 || fromIndex >= s.Length)
            {
                found = null;
                return false;
            }

            found = any.Select(x => new Tuple<string, int>(x, s.IndexOf(x, fromIndex)))
                       .Where(t => t.Item2 > -1)
                       .OrderBy(t => t.Item2)
                       .FirstOrDefault();
            return found != null;
        }

        /// <summary>
        /// Removes eagerly characters from end of the string.
        /// Typical usage is <c>RemoveFromEnd(Path.DirectorySeparatorChar)</c>.
        /// </summary>
        public static string RemoveFromEnd(this string s, params char[] chars)
        {
            if (string.IsNullOrEmpty(s) || chars == null || chars.Length == 0)
            {
                return s;
            }

            var lastPos = -1;
            for (var i = s.Length - 1; i >= 0; i--)
            {
                if (!chars.Contains(s[i]))
                {
                    break;
                }

                lastPos = i;
            }

            if (lastPos == 0)
            {
                return string.Empty;
            }

            return lastPos > 0
                ? s.Substring(0, lastPos)
                : s;
        }

        /// <summary>
        /// Removes any non-alphanumeric characters and formats the string to use pascal casing.
        /// It is often use to create safe names which does not require any escaping in URLs and other places.
        /// Pascal casing: &quot;Lazy dog jumped!&quot; =&gt; LazyDogJumped
        /// </summary>
        public static string PascalCase(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            var s1 = WhiteSpaceRx.Replace(s, "_");
            s1 = NonSafeRx.Replace(s1, string.Empty);
            var parts = s1.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
                          .Select(x => x.Substring(0, 1).ToUpperInvariant() + x.Substring(1));
            return string.Join(string.Empty, parts);
        }

        public static string SubstrUntilFirst(this string s, char lookup)
        {
            var idx = s.IndexOf(lookup);
            if (idx == -1)
            {
                return s;
            }

            return s.Substring(0, idx);
        }

        public static string SubstrFromLast(this string s, char lookup)
        {
            var idx = s.LastIndexOf(lookup);
            if (idx == -1)
            {
                return s;
            }

            return s.Substring(idx + 1);
        }

        private static bool TryParse(string s, out TimeSpan ts)
        {
            var m = TimeSpanRgx.Match(s);
            if (m.Success)
            {
                var num = double.Parse(m.Groups["num"].Value);
                var unit = m.Groups["unit"].Value.ToLowerInvariant();
                switch (unit)
                {
                    case "ms":
                    case "millis":
                    case "millisecond":
                    case "milliseconds":
                        ts = TimeSpan.FromMilliseconds(num);
                        return true;

                    case "s":
                    case "sec":
                    case "secs":
                    case "second":
                    case "seconds":
                        ts = TimeSpan.FromSeconds(num);
                        return true;

                    case "m":
                    case "min":
                    case "mins":
                    case "minute":
                    case "minutes":
                        ts = TimeSpan.FromMinutes(num);
                        return true;

                    case "h":
                    case "hrs":
                    case "hour":
                    case "hours":
                        ts = TimeSpan.FromHours(num);
                        return true;

                    case "d":
                    case "day":
                    case "days":
                        ts = TimeSpan.FromDays(num);
                        return true;
                }
            }

            ts = TimeSpan.Zero;
            return false;
        }
    }
}
