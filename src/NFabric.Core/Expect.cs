namespace NFabric.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Collection of utility methods to check method arguments while claiming as little as possible of vertical screen space.
    /// Improves readability quite a bit.
    /// All methods throw <see cref="ArgumentException"/> or its derivatives.
    /// </summary>
    public static class Expect
    {
        private static readonly char[] NewLineChars = { '\r', '\n' };

        private static readonly Regex AlphaNumRgx = new Regex("^[a-z0-9]$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex SafeNameRgx = new Regex(@"^[a-z]\w*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotNull(object arg, string argName)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotEmpty(string arg, string argName)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argName);
            }

            if (string.IsNullOrEmpty(arg))
            {
                throw new ArgumentException($"argument '{argName}' must not be empty string", argName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Regex(Regex rx, string arg, string argName)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            if (arg == null)
            {
                throw new ArgumentNullException(argName);
            }

            if (!rx.IsMatch(arg))
            {
                throw new ArgumentException($"argument '{argName}' must conform to regular expression {rx}", argName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SafeName(string arg, string argName)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argName);
            }

            if (!SafeNameRgx.IsMatch(arg))
            {
                throw new ArgumentException($"argument '{argName}' is not a safe name", argName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AlphaNum(string arg, string argName)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argName);
            }

            if (!AlphaNumRgx.IsMatch(arg))
            {
                throw new ArgumentException($"argument '{argName}' contains other than alpha-numeric characters", argName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotEmpty<T>(ICollection<T> arg, string argName)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argName);
            }

            if (arg.Count == 0)
            {
                throw new ArgumentException($"argument '{argName}' must not be empty {arg.GetType().Name}", argName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Positive(int arg, string argName)
        {
            if (arg < 1)
            {
                throw new ArgumentException($"argument '{argName}' must be greater than 0", argName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Positive(long arg, string argName)
        {
            if (arg < 1)
            {
                throw new ArgumentException($"argument '{argName}' must be greater than 0", argName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotDefault<T>(T arg, string argName)
        {
            if (arg.Equals(default(T)))
            {
                throw new ArgumentException($"argument '{argName}' must not equal default value for {arg.GetType().Name}", argName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Range<T>(T arg, T min, T max, string argName)
            where T : IComparable<T>
        {
            if (arg.CompareTo(min) == -1 || arg.CompareTo(max) == 1)
            {
                throw new ArgumentException($"argument '{argName}' must within between {min} and {max} (both inclusive)", argName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotInclude(char[] exclusions, string arg, string argName)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argName);
            }

            if (exclusions == null || exclusions.Length == 0)
            {
                return;
            }

            if (arg.IndexOfAny(exclusions) >= 0)
            {
                throw new ArgumentException(argName, $"argument '{argName}' must not contains excluded characters");
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotIncludeNewLine(string arg, string argName)
        {
            NotInclude(NewLineChars, arg, argName);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FileExists(string arg, string argName)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argName);
            }

            if (!File.Exists(arg))
            {
                throw new ArgumentException(argName, $"file '{arg}' does not exists");
            }
        }

        [DebuggerStepThrough]
        public static void NotDisposed(bool isDisposed, [CallerMemberName] string callerMemberName = "")
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException($"calling method '{callerMemberName}' of disposed object is not allowed");
            }
        }
    }
}
