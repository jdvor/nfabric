namespace NFabric.Core.Extensions
{
    using System;

    [Flags]
    public enum Base64Options
    {
        Default = 1,

        NoPadding = 1 << 1,

        UrlSafe = 1 << 2,
    }
}
