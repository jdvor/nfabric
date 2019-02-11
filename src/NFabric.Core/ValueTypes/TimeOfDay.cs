namespace NFabric.Core.ValueTypes
{
    using NFabric.Core;
    using JetBrains.Annotations;
    using System;
    using System.Text.RegularExpressions;

    public struct TimeOfDay : IEquatable<TimeOfDay>, IComparable<TimeOfDay>
    {
        public static readonly Regex Regex = new Regex(@"^(([0-1]\d)|(2[0-3]))\d:[0-5]\d$", RegexOptions.Compiled);

        private short TotalMinutes { get; }

        public static readonly TimeOfDay Empty = new TimeOfDay(-1);

        public bool IsEmpty => Equals(Empty);

        public TimeOfDay(short value)
        {
            Expect.Range(value, -1, 2359, nameof(value));
            TotalMinutes = value;
        }

        public TimeOfDay([NotNull] string value)
        {
            Expect.Regex(Regex, value, nameof(value));
            TotalMinutes = short.Parse(value.Replace(":", string.Empty));
        }

        public bool Equals(TimeOfDay other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (ReferenceEquals(this, null) || ReferenceEquals(other, null))
            {
                return false;
            }

            return TotalMinutes == other.TotalMinutes;
        }

        public override bool Equals(object obj)
        {
            if (obj is TimeOfDay)
            {
                return Equals((TimeOfDay)obj);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return TotalMinutes.GetHashCode();
        }

        public static bool operator ==(TimeOfDay left, TimeOfDay right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TimeOfDay left, TimeOfDay right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            if (TotalMinutes <= 59)
            {
                return $"00:{TotalMinutes:00}";
            }

            var h = TotalMinutes / 100;
            var m = TotalMinutes % 100;

            return $"{h:00}:{m:00}";
        }

        public int CompareTo(TimeOfDay other)
        {
            return TotalMinutes.CompareTo(other.TotalMinutes);
        }

        public static bool operator <(TimeOfDay left, TimeOfDay right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(TimeOfDay left, TimeOfDay right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(TimeOfDay left, TimeOfDay right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(TimeOfDay left, TimeOfDay right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}
