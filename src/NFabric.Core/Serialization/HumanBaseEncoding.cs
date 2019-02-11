namespace NFabric.Core.Serialization
{
    using NFabric.Core.Extensions;
    using System;
    using System.Globalization;
    using System.Linq;

    public sealed class HumanBaseEncoding
    {
        private const int BitsPerByte = 8;
        private readonly char[] charBank;
        private readonly int bitsPerChar;
        private readonly int separateEvery;
        private readonly char separator;

        public static readonly Lazy<HumanBaseEncoding> Instance = new Lazy<HumanBaseEncoding>(() => new HumanBaseEncoding());

        public HumanBaseEncoding()
        {
            charBank = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ".ToCharArray();
            bitsPerChar = 5;
            separateEvery = 6;
            separator = '-';
        }

        public string Encode(byte[] data, bool withSeparators = false)
        {
            var bitArray = new BitArray(data, BitsPerByte);
            var encodedLength = (bitArray.Length / bitsPerChar) + (bitArray.Length % bitsPerChar == 0 ? 0 : 1);

            var result = new char[encodedLength];
            for (var i = 0; i < encodedLength; i++)
            {
                var value = 0;
                for (var j = bitsPerChar - 1; j >= 0; j--)
                {
                    var index = ((i + 1) * bitsPerChar) - j - 1;
                    if (index >= bitArray.Length)
                    {
                        // We might run out of bounds of the array in the end of the message,
                        // that's ok, just write to the output whatever we've got so far.
                        break;
                    }

                    value |= bitArray[index] << j;
                }

                result[i] = charBank[value];
            }

            return withSeparators
                ? WithSeparators(result)
                : new string(result);
        }

        public byte[] Decode(string str)
        {
            if (!IsLengthAndCharsOk(str))
            {
                throw new ArgumentException("Invalid characters or length", nameof(str));
            }

            var data = str
                .Trim()
                .Replace(separator.ToString(CultureInfo.InvariantCulture), string.Empty)
                .Select(ch => (byte)Array.IndexOf(charBank, ch))
                .ToArray();
            var bitArray = new BitArray(data, bitsPerChar);

            var result = new byte[bitArray.Length / BitsPerByte];
            for (var i = 0; i < result.Length; i++)
            {
                var value = 0;
                for (var j = BitsPerByte - 1; j >= 0; j--)
                {
                    var index = ((i + 1) * BitsPerByte) - j - 1;
                    if (index >= bitArray.Length)
                    {
                        break;
                    }

                    value |= bitArray[index] << j;
                }

                result[i] = (byte)value;
            }

            return result;
        }

        private bool IsLengthAndCharsOk(string str)
        {
            if (str == null)
            {
                return false;
            }

            var strLenWithoutSeps = str.Count(ch => ch != separator);

            int n;
            for (var i = 1; (n = (int)Math.Ceiling(i * BitsPerByte / (decimal)bitsPerChar)) < strLenWithoutSeps; i++)
            {
            }

            if (strLenWithoutSeps != n)
            {
                return false;
            }

            return !str.Any(ch => ch != separator && Array.IndexOf(charBank, ch) < 0);
        }

        private string WithSeparators(char[] chars)
        {
            var s = new string(chars);
            return chars.Length > separateEvery
                ? s.Interleave(separator.ToString(CultureInfo.InvariantCulture), separateEvery)
                : s;
        }

        private class BitArray
        {
            private readonly byte[] data;

            private readonly int bitsPerByte;

            public BitArray(byte[] data, int bitsPerByte)
            {
                this.data = data;
                this.bitsPerByte = bitsPerByte;
            }

            public int Length
            {
                get { return data.Length * bitsPerByte; }
            }

            public int this[int index]
            {
                get
                {
                    var i = index / bitsPerByte;
                    var shift = bitsPerByte - (index % bitsPerByte) - 1;
                    return (data[i] & (1 << shift)) >> shift;
                }
            }
        }
    }
}
