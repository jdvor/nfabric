namespace NFabric.Core.Tests
{
    using NFabric.Core.Extensions;
    using System.Collections.Generic;
    using Xunit;

    public class BytesExtensionsTests
    {
        [Theory]
        [MemberData(nameof(ByteArraysTestData))]
        public void ToAndFromBase64(byte[] bytes)
        {
            var enc1 = bytes.ToBase64String();
            var dec1 = enc1.AsBytesFromBase64();
            Assert.True(dec1.IsSameAs(bytes));

            var enc2 = bytes.ToBase64String(Base64Options.NoPadding);
            var dec2 = enc2.AsBytesFromBase64(Base64Options.NoPadding);
            Assert.DoesNotContain("=", enc2);
            Assert.True(dec2.IsSameAs(bytes));

            var enc3 = bytes.ToBase64String(Base64Options.UrlSafe);
            var dec3 = enc3.AsBytesFromBase64(Base64Options.UrlSafe);
            Assert.DoesNotContain("=", enc3);
            Assert.DoesNotContain("/", enc3);
            Assert.DoesNotContain("+", enc3);
            Assert.True(dec3.IsSameAs(bytes));
        }

        public static IEnumerable<object[]> ByteArraysTestData()
        {
            yield return new object[] { new byte[0] };
            yield return new object[] { new byte[] { 1 } };
            yield return new object[] { new byte[] { 1, 2 } };
            yield return new object[] { new byte[] { 1, 2, 3 } };
            yield return new object[] { new byte[] { 1, 2, 3, 254 } };
            yield return new object[] { new byte[] { 1, 2, 3, 254, 255 } };
        }
    }
}
