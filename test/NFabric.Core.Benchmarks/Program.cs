namespace NFabric.Core.Benchmarks
{
    using System;
    using System.Security.Cryptography;

    public static class Program
    {
        public static void Main(string[] args)
        {
            Randoms();

            Console.WriteLine("\r\n\r\nPress any key to exit...");
            Console.ReadKey();
        }

        public static void Randoms()
        {
            const int cnt = 1000000;
            var benchmark = new Benchmark("Random bytes generation", runs: 3);

            var tlr = Security.Util.CreateThreadLocalRandom();
            benchmark.MeasureElapsedMilliseconds("ThreadLocal<Random>", () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    var _ = new byte[16];
                    tlr.Value.NextBytes(_);
                }
            });

            var rng = new RNGCryptoServiceProvider();
            benchmark.MeasureElapsedMilliseconds("RNGCryptoServiceProvider", () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    var _ = new byte[16];
                    rng.GetBytes(_);
                }
            });

            var rb = new Security.RandomBytes(1600);
            benchmark.MeasureElapsedMilliseconds("RandomBytes", () =>
            {
                for (int i = 0; i < cnt; i++)
                {
                    var _ = rb.GetBytes(16);
                }
            });

            benchmark.MeasureAndOutputToConsole();
        }
    }
}
