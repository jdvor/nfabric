namespace NFabric.Core.Tests
{
    using NFabric.Core.ValueTypes;
    using System;
    using Xunit;

    public class RichTokenTests
    {
        [Fact]
        public void FromAndToString()
        {
            var payload = new byte[] { 3, 15, 254 };
            var issued = new DateTimeOffset(2018, 10, 9, 14, 21, 0, TimeSpan.Zero);
            var rt = RichToken.Create(payload, 0, issued);
            var str = rt.ToString();

            var ok = RichToken.TryParse(str, out RichToken rt2);

            Assert.True(ok);
            Assert.True(rt.Equals(rt2));
        }

        [Fact]
        public void CtorThrowsOnEmpty()
        {
            Assert.ThrowsAny<ArgumentException>(() => new RichToken(Array.Empty<byte>()));
        }

        [Fact]
        public void CtorThrowsOnNull()
        {
            Assert.ThrowsAny<ArgumentException>(() => new RichToken(null));
        }
    }
}
