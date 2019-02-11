namespace NFabric.Core.ValueTypes
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Plus / minus <a href="http://en.wikipedia.org/wiki/ISO_8601#Durations">ISO 8601</a> duration including time;
    /// that is only years, months, days, hours, minutes and seconds.
    /// </summary>
    /// <example>
    /// P1Y ... one year
    /// P6M ... six months
    /// P2Y90D ... two years and sixty days
    /// </example>
    /// <remarks>
    /// There can be ArithmeticOverflow exception thrown when comparing instances where years or months are very high numbers.
    /// </remarks>
    public sealed class CoarseDuration : IEquatable<CoarseDuration>, IComparable<CoarseDuration>
    {
        public static readonly CoarseDuration Empty = new CoarseDuration();

        #pragma warning disable SA1117 // Parameters should be on same line or separate lines
        public static readonly string PatternStr = string.Format(
            @"^(?<{7}>\{8})?P((?<{0}>\d+){0})?((?<{1}>\d+){1})?((?<{2}>\d+){2})?(?:T((?<{3}>\d+){3})?((?<{5}>\d+){4})?((?<{6}>\d+){6})?)?$",
            YearHint, MonthHint, DayHint, HourHint, MinuteHint, MinuteGroup, SecondHint, SignHint, MinusSign);
        #pragma warning restore SA1117

        public static readonly Regex Pattern = new Regex(PatternStr, RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private const string YearHint = "Y";
        private const string MonthHint = "M";
        private const string DayHint = "D";
        private const string HourHint = "H";
        private const string MinuteHint = "M";
        private const string MinuteGroup = "MN";
        private const string SecondHint = "S";
        private const string SignHint = "SGN";
        private const string MinusSign = "-";
        private const int DaysPerYear = 365;
        private const int DaysPerMonth = 30;
        private const int SecondsPerMinute = 60;
        private const int SecondsPerHour = 60 * SecondsPerMinute;
        private const int SecondsPerDay = 24 * SecondsPerHour;

        public uint Years { get; }

        public uint Months { get; }

        public uint Days { get; }

        public uint Hours { get; }

        public uint Minutes { get; }

        public uint Seconds { get; }

        public Sign Sign { get; }

        /// <summary>
        /// Gets total seconds of this duration instance.
        /// </summary>
        public uint TotalSeconds => (((Years * DaysPerYear) + (Months * DaysPerMonth) + Days) * SecondsPerDay) + (Hours * SecondsPerHour) + (Minutes * SecondsPerMinute) + Seconds;

        public CoarseDuration(uint years, uint months, uint days)
        {
            Years = years;
            Months = months;
            Days = days;
            Sign = Sign.Plus;
        }

        public CoarseDuration(Sign sign = Sign.Plus, uint years = 0, uint months = 0, uint days = 0, uint hours = 0, uint minutes = 0, uint seconds = 0)
        {
            Expect.NotDefault(sign, nameof(sign));

            Years = years;
            Months = months;
            Days = days;
            Hours = hours;
            Minutes = minutes;
            Seconds = seconds;
            Sign = sign;
        }

        public CoarseDuration(string durationOnlyIso8601)
        {
            if (string.IsNullOrEmpty(durationOnlyIso8601))
            {
                throw new ArgumentNullException(nameof(durationOnlyIso8601), "The null or empty string value are not acceptable.");
            }

            var match = Pattern.Match(durationOnlyIso8601);
            if (!match.Success)
            {
                throw new ArgumentException("The string value has unexpected format.", nameof(durationOnlyIso8601));
            }

            try
            {
                var yg = match.Groups[YearHint];
                if (!string.IsNullOrEmpty(yg?.Value))
                {
                    Years = uint.Parse(yg.Value, NumberStyles.Integer);
                }

                var mg = match.Groups[MonthHint];
                if (!string.IsNullOrEmpty(mg?.Value))
                {
                    Months = uint.Parse(mg.Value, NumberStyles.Integer);
                }

                var dg = match.Groups[DayHint];
                if (!string.IsNullOrEmpty(dg?.Value))
                {
                    Days = uint.Parse(dg.Value, NumberStyles.Integer);
                }

                var hg = match.Groups[HourHint];
                if (!string.IsNullOrEmpty(hg?.Value))
                {
                    Hours = uint.Parse(hg.Value, NumberStyles.Integer);
                }

                var mng = match.Groups[MinuteGroup];
                if (!string.IsNullOrEmpty(mng?.Value))
                {
                    Minutes = uint.Parse(mng.Value, NumberStyles.Integer);
                }

                var sg = match.Groups[SecondHint];
                if (!string.IsNullOrEmpty(sg?.Value))
                {
                    Seconds = uint.Parse(sg.Value, NumberStyles.Integer);
                }

                var sgn = match.Groups[SignHint];

                Sign = (!string.IsNullOrEmpty(sgn?.Value) && sgn.Value.Equals(MinusSign)) ? Sign.Minus : Sign.Plus;
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("The string value has unexpected format.", nameof(durationOnlyIso8601), ex);
            }
        }

        public CoarseDuration(TimeSpan timeSpan)
        {
            if (timeSpan.Days != 0)
            {
                Days = Convert.ToUInt32(Math.Abs(timeSpan.Days));
            }

            if (timeSpan.Hours != 0)
            {
                Hours = Convert.ToUInt32(Math.Abs(timeSpan.Hours));
            }

            if (timeSpan.Minutes != 0)
            {
                Minutes = Convert.ToUInt32(Math.Abs(timeSpan.Minutes));
            }

            if (timeSpan.Seconds != 0)
            {
                Seconds = Convert.ToUInt32(Math.Abs(timeSpan.Seconds));
            }

            if (timeSpan.Milliseconds != 0)
            {
                Seconds += Convert.ToUInt32(Math.Ceiling(Math.Abs(timeSpan.Milliseconds) / 1000d));
            }

            Sign = timeSpan < TimeSpan.Zero ? Sign.Minus : Sign.Plus;
        }

        public static Tuple<bool, string> Validate(string durationOnlyIso8601)
        {
            if (string.IsNullOrEmpty(durationOnlyIso8601))
            {
                return new Tuple<bool, string>(false, "CoarseDuration (ctor value) must not be null or empty");
            }

            var match = Pattern.Match(durationOnlyIso8601);
            if (!match.Success)
            {
                var errMsg = $"CoarseDuration (ctor value '{durationOnlyIso8601}') does not match pattern '{PatternStr}'";
                return new Tuple<bool, string>(false, errMsg);
            }

            return new Tuple<bool, string>(true, null);
        }

        public bool Equals(CoarseDuration other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Years.Equals(other.Years)
                && Months.Equals(other.Months)
                && Days.Equals(other.Days)
                && Hours.Equals(other.Hours)
                && Minutes.Equals(other.Minutes)
                && Seconds.Equals(other.Seconds)
                && Sign.Equals(other.Sign);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CoarseDuration);
        }

        public int CompareTo(CoarseDuration other)
        {
            if (ReferenceEquals(other, null))
            {
                return 1;
            }

            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            var thisEstimate = (Sign == Sign.Minus ? -1 : 1) * TotalSeconds;
            var otherEstimate = (other.Sign == Sign.Minus ? -1 : 1) * other.TotalSeconds;

            return (thisEstimate > otherEstimate) ? 1 : (thisEstimate < otherEstimate) ? -1 : 0;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // (N ^ (N << 8)) ... better normal distribution of the hash
                // when do you expect N to be from a very narrow range and not the whole offered by the data type.

                uint h = 17;
                h = (23 * h) + (Years ^ (Years << 8));
                h = (23 * h) + (Months ^ (Months << 8));
                h = (23 * h) + (Days ^ (Days << 8));
                h = (23 * h) + (Hours ^ (Hours << 8));
                h = (23 * h) + (Minutes ^ (Minutes << 8));
                h = (23 * h) + (Seconds ^ (Seconds << 8));
                h = (23 * h) + (Convert.ToUInt32(Sign) ^ (Convert.ToUInt32(Sign) << 8));
                return (int)h;
            }
        }

        public TimeSpan GetAsTimeSpan()
        {
            return new TimeSpan(
                Convert.ToInt32((Years * DaysPerYear) + (Months * DaysPerMonth) + Days),
                Convert.ToInt32(Hours),
                Convert.ToInt32(Minutes),
                Convert.ToInt32(Seconds));
        }

        public override string ToString()
        {
            var sb = new StringBuilder(21);

            if (Sign == Sign.Minus)
            {
                sb.Append(MinusSign);
            }

            sb.Append("P");

            if (Years > 0)
            {
                sb.Append(Years);
                sb.Append(YearHint);
            }

            if (Months > 0)
            {
                sb.Append(Months);
                sb.Append(MonthHint);
            }

            if (Days > 0)
            {
                sb.Append(Days);
                sb.Append(DayHint);
            }

            if (Hours > 0 || Minutes > 0 || Seconds > 0)
            {
                sb.Append("T");

                if (Hours > 0)
                {
                    sb.Append(Hours);
                    sb.Append(HourHint);
                }

                if (Minutes > 0)
                {
                    sb.Append(Minutes);
                    sb.Append(MinuteHint);
                }

                if (Seconds > 0)
                {
                    sb.Append(Seconds);
                    sb.Append(SecondHint);
                }
            }

            return sb.ToString();
        }

        private static int Compare(CoarseDuration lhs, CoarseDuration rhs)
        {
            if (ReferenceEquals(lhs, null))
            {
                return ReferenceEquals(rhs, null) ? 0 : -1;
            }

            return lhs.CompareTo(rhs);
        }

        #region Operator Overloads

        public static bool operator ==(CoarseDuration lhs, CoarseDuration rhs)
        {
            if (ReferenceEquals(lhs, null))
            {
                return ReferenceEquals(rhs, null);
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(CoarseDuration lhs, CoarseDuration rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator <(CoarseDuration lhs, CoarseDuration rhs)
        {
            return Compare(lhs, rhs) < 0;
        }

        public static bool operator <=(CoarseDuration lhs, CoarseDuration rhs)
        {
            return Compare(lhs, rhs) <= 0;
        }

        public static bool operator >(CoarseDuration lhs, CoarseDuration rhs)
        {
            return Compare(lhs, rhs) > 0;
        }

        public static bool operator >=(CoarseDuration lhs, CoarseDuration rhs)
        {
            return Compare(lhs, rhs) >= 0;
        }

        public static CoarseDuration operator +(CoarseDuration lhs, CoarseDuration rhs)
        {
            var lhss = lhs.Sign == Sign.Minus ? -1 : 1;
            var rhss = rhs.Sign == Sign.Minus ? -1 : 1;
            var tts = (lhss * lhs.TotalSeconds) + (rhss * rhs.TotalSeconds);

            return FromSeconds(tts);
        }

        public static CoarseDuration operator -(CoarseDuration lhs, CoarseDuration rhs)
        {
            var lhss = lhs.Sign == Sign.Minus ? -1 : 1;
            var rhss = rhs.Sign == Sign.Minus ? -1 : 1;
            var tts = (lhss * lhs.TotalSeconds) - (rhss * rhs.TotalSeconds);

            return FromSeconds(tts);
        }

        private static CoarseDuration FromSeconds(long tts)
        {
            var rdays = Math.Floor((double)Math.Abs(tts) / SecondsPerDay);

            var years = Math.Floor(rdays / DaysPerYear);
            var months = Math.Floor((rdays - (years * DaysPerYear)) / DaysPerMonth);
            var days = rdays - (years * DaysPerYear) - (months * DaysPerMonth);

            var srest = Math.Abs(tts) - (rdays * SecondsPerDay);
            var hours = Math.Floor(srest / SecondsPerHour);
            var minutes = Math.Floor((srest - (hours * SecondsPerHour)) / SecondsPerMinute);
            var seconds = srest - (hours * SecondsPerHour) - (minutes * SecondsPerMinute);

            return new CoarseDuration(
                tts < 0 ? Sign.Minus : Sign.Plus,
                Convert.ToUInt32(years),
                Convert.ToUInt32(months),
                Convert.ToUInt32(days),
                Convert.ToUInt32(hours),
                Convert.ToUInt32(minutes),
                Convert.ToUInt32(seconds));
        }

        public static implicit operator string(CoarseDuration duration)
        {
            if (ReferenceEquals(duration, null))
            {
                return null;
            }

            return duration.ToString();
        }

        #endregion
    }
}
