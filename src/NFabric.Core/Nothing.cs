namespace NFabric.Core
{
    using System;

    /// <summary>
    /// Used as replacement for metric timers (they use using block semantic) when no metrics system is available.
    /// Or to signal HTTP response body does not have to be really read from.
    /// </summary>
    public sealed class Nothing : IDisposable
    {
        private Nothing()
        {
        }

        public void Dispose()
        {
        }

        public static readonly Nothing Instance = new Nothing();
    }
}
