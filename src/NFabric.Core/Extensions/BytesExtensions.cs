namespace NFabric.Core.Extensions
{
    using NFabric.Core.Serialization;
    using System;

    public static class BytesExtensions
    {
        public static string ToHexString(this byte[] bytes)
        {
            return bytes?.Length > 0
                ? BitConverter.ToString(bytes).Replace("-", string.Empty)
                : string.Empty;
        }

        public static string ToBase64String(this byte[] bytes, Base64Options options = Base64Options.Default)
        {
            if (bytes?.Length == 0)
            {
                return string.Empty;
            }

            var s = Convert.ToBase64String(bytes);

            if (options.HasFlag(Base64Options.NoPadding) || options.HasFlag(Base64Options.UrlSafe))
            {
                var idx = s.IndexOf('=');
                if (idx > 0)
                {
                    s = s.Substring(0, idx);
                }
            }

            if (options.HasFlag(Base64Options.UrlSafe))
            {
                s = s.Replace('/', '_').Replace('+', '-');
            }

            return s;
        }

        public static string ToHumanBaseString(this byte[] bytes, bool withSeparators = false)
        {
            return HumanBaseEncoding.Instance.Value.Encode(bytes, withSeparators);
        }

        /// <summary>
        /// https://stackoverflow.com/questions/43289/comparing-two-byte-arrays-in-net
        /// </summary>
        public static bool IsSameAs(this byte[] bytes, byte[] otherBytes)
        {
            if (ReferenceEquals(bytes, otherBytes))
            {
                return true;
            }

            if (ReferenceEquals(bytes, null) ||
                ReferenceEquals(otherBytes, null) ||
                bytes.Length != otherBytes.Length)
            {
                return false;
            }

            int i = 0;
            while (i < bytes.Length)
            {
                if (bytes[i] != otherBytes[i])
                {
                    return false;
                }

                ++i;
            }

            return true;
        }
    }
}
