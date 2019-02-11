namespace NFabric.Core.Benchmarks
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    public class Benchmark
    {
        private readonly string benchmarkName;
        private readonly int runs;
        private readonly bool forceGc;
        private readonly int wait;
        private readonly Dictionary<string, Func<double>> actions;

        public Benchmark(string name, int runs, bool forceGc = false, int wait = 0)
        {
            benchmarkName = name;
            this.runs = runs;
            this.forceGc = forceGc;
            this.wait = wait;
            actions = new Dictionary<string, Func<double>>();
        }

        public void AddMeasurement(string name, Func<double> action)
        {
            actions.Add(name, action);
        }

        public void MeasureElapsedMilliseconds(string name, Action action)
        {
            AddMeasurement(name, () =>
            {
                var sw = Stopwatch.StartNew();
                action();
                return sw.ElapsedMilliseconds;
            });
        }

        public Dictionary<string, double[]> MeasureRaw()
        {
            var metrics = actions.ToDictionary(kvp => kvp.Key, _ => new double[runs]);
            for (int i = 0; i < runs; i++)
            {
                foreach (var kvp in actions)
                {
                    metrics[kvp.Key][i] = kvp.Value();

                    if (wait > 0)
                    {
                        Thread.Sleep(wait);
                    }

                    if (forceGc)
                    {
                        ForceGC();
                    }
                }
            }

            return metrics;
        }

        public (string Name, double Avg, double Ratio)[] Measure()
        {
            var metrics = MeasureRaw();
            var averages = metrics.ToDictionary(x => x.Key, x => x.Value.Average());
            var baseline = averages.Values.Min();
            return averages
                .Select(x => (Name: x.Key, Avg: x.Value, Ratio: Math.Round(x.Value / baseline, 2)))
                .OrderByDescending(x => x.Avg)
                .ToArray();
        }

        public void MeasureAndOutputToConsole()
        {
            var metrics = Measure();
            Console.WriteLine($"{benchmarkName} (runs: {runs}, gc: {forceGc}, wait: {wait})");
            Console.WriteLine("-------------------------------------------------------");
            foreach (var (name, avg, ratio) in metrics)
            {
                Console.WriteLine($"{name,-30} {ratio:F2} {avg:F2}");
            }
            Console.WriteLine("-------------------------------------------------------");
        }

        private static void ForceGC()
        {
            GC.Collect(2, GCCollectionMode.Forced, blocking: true);
            GC.WaitForPendingFinalizers();
        }
    }
}
