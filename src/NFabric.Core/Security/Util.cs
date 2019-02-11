namespace NFabric.Core.Security
{
    using System;
    using System.Threading;

    public static class Util
    {
        public static ThreadLocal<Random> CreateThreadLocalRandom()
        {
            return new ThreadLocal<Random>(() => new Random(Environment.TickCount ^ Thread.CurrentThread.ManagedThreadId));
        }
    }
}
