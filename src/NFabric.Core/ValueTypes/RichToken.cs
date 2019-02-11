namespace NFabric.Core.ValueTypes
{
    using NFabric.Core;
    using NFabric.Core.Extensions;
    using NFabric.Core.Serialization;
    using System;
    using System.Threading;

    public struct RichToken : IEquatable<RichToken>
    {
        private const int TimeSize = 8;
        private const int LenSize = 2;
        private const int MinBytesLength = TimeSize + LenSize + 1;
        private const long MinTime = 946681200000; // 2000-01-01 (in milliseconds)
        private const long MaxTime = 4102441200000; // 2100-01-01 (in milliseconds)
        private static readonly HumanBaseEncoding Encoding = new HumanBaseEncoding();
        private static readonly ThreadLocal<Random> Rand = Security.Util.CreateThreadLocalRandom();

        /// <summary>
        /// Array structure, where 'x' is bytes total length:
        /// [offset]      value description
        /// --------------------------------------------------------------------
        /// [0]           random bytes (optional, when x = PL + 11)
        /// [x - 11 - PL] payload bytes
        /// [x - 11]      PL, payload bytes length (Int16, 2 bytes before time)
        /// [x - 9]       time (Int64, 8 bytes at the end)
        /// --------------------------------------------------------------------
        /// </summary>
        private readonly byte[] value;

        private readonly Lazy<string> valueAsString;

        public static readonly RichToken Empty = CreateEmpty();

        public bool IsEmpty => Equals(Empty);

        public ArraySegment<byte> Payload { get; }

        public DateTimeOffset? Issued { get; }

        public RichToken(byte[] value)
        {
            Expect.NotEmpty(value, nameof(value));

            if (TryReadBytes(value, out ArraySegment<byte> payload, out DateTimeOffset? issued))
            {
                this.value = value;
                Payload = payload;
                Issued = issued;
                valueAsString = new Lazy<string>(() => Encoding.Encode(value, withSeparators: false));
                return;
            }

            throw new ArgumentException($"Bytes could not be converted into valid {nameof(RichToken)}.", nameof(value));
        }

        private RichToken(byte[] value, ArraySegment<byte> payload, DateTimeOffset? issued)
        {
            this.value = value;
            Payload = payload;
            Issued = issued;
            valueAsString = value.Length > 0
                ? new Lazy<string>(() => Encoding.Encode(value, withSeparators: false))
                : new Lazy<string>(() => string.Empty);
        }

        public bool Equals(RichToken other)
        {
            return value.IsSameAs(other.value);
        }

        public override bool Equals(object obj)
        {
            if (obj is RichToken)
            {
                return Equals((RichToken)obj);
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int h = 0;
                for (int i = 0; i < value.Length; i++)
                {
                    h = (h * 17) + value[i].GetHashCode();
                }

                return h;
            }
        }

        public static bool operator ==(RichToken left, RichToken right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RichToken left, RichToken right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return valueAsString.Value;
        }

        public static implicit operator string(RichToken token)
        {
            return token.valueAsString.Value;
        }

        public static bool TryParse(string s, out RichToken token)
        {
            try
            {
                var bytes = Encoding.Decode(s);
                token = new RichToken(bytes);
                return true;
            }
            catch
            {
                token = Empty;
                return false;
            }
        }

        public static bool TryReadBytes(byte[] bytes, out ArraySegment<byte> payload, out DateTimeOffset? issued)
        {
            if (bytes?.Length < MinBytesLength)
            {
                payload = new ArraySegment<byte>(Array.Empty<byte>());
                issued = null;
                return false;
            }

            var timeStartsAt = bytes.Length - TimeSize;
            var unixMillis = BitConverter.ToInt64(bytes, timeStartsAt);
            if (!(unixMillis >= MinTime && unixMillis <= MaxTime))
            {
                payload = new ArraySegment<byte>(Array.Empty<byte>());
                issued = null;
                return false;
            }

            var payloadLengthStartsAt = timeStartsAt - LenSize;
            var payloadLength = BitConverter.ToInt16(bytes, payloadLengthStartsAt);
            if (payloadLength > bytes.Length - LenSize - TimeSize)
            {
                payload = new ArraySegment<byte>(Array.Empty<byte>());
                issued = null;
                return false;
            }

            var payloadStartsAt = payloadLengthStartsAt - payloadLength;
            payload = new ArraySegment<byte>(bytes, payloadStartsAt, payloadLength);
            issued = DateTimeOffset.FromUnixTimeMilliseconds(unixMillis);
            return true;
        }

        private static RichToken CreateEmpty()
        {
            var v = Array.Empty<byte>();
            return new RichToken(v, new ArraySegment<byte>(v), null);
        }

        public static RichToken Create(byte[] payload, int prefixSize = 0, DateTimeOffset? now = null)
        {
            Expect.Range(payload.Length, 1, 100, nameof(payload));
            Expect.Range(prefixSize, 0, 100, nameof(payload));

            var payloadSize = payload.Length;
            var value = new byte[prefixSize + payloadSize + LenSize + TimeSize];

            if (prefixSize > 0)
            {
                var prefixBytes = new byte[prefixSize];
                Rand.Value.NextBytes(prefixBytes);
                Buffer.BlockCopy(prefixBytes, 0, value, 0, prefixSize);
            }

            var payloadStartsAt = prefixSize;
            Buffer.BlockCopy(payload, 0, value, payloadStartsAt, payloadSize);

            var payloadLengthBytes = BitConverter.GetBytes((short)payloadSize);
            var payloadLengthStartsAt = payloadStartsAt + payloadSize;
            Buffer.BlockCopy(payloadLengthBytes, 0, value, payloadLengthStartsAt, LenSize);

            var issued = now ?? DateTimeOffset.UtcNow;
            var ms = issued.ToUnixTimeMilliseconds();
            var timeBytes = BitConverter.GetBytes(ms);
            var timeStartsAt = payloadLengthStartsAt + LenSize;
            Buffer.BlockCopy(timeBytes, 0, value, timeStartsAt, TimeSize);

            return new RichToken(value, new ArraySegment<byte>(value, payloadStartsAt, payloadSize), issued);
        }
    }
}
