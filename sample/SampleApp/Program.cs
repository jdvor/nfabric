namespace SampleApp
{
    using NFabric.Host;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args?.Length > 0)
            {
                SingleActionRunner(args);
            }
            else
            {
                // LongRunner(args);
                StartStopRunner(args);
            }
        }

        private static async Task DoAsync(int n, CancellationToken ct)
        {
            await Task.Delay(150).ConfigureAwait(false);
            Console.WriteLine($"{n} ({Thread.CurrentThread.ManagedThreadId})");
        }

        private static void LongRunner(string[] args)
        {
            var runner = new RunnerBuilder()
                            .UseCommandLineArgs(args)
                            .UseConfig("appsettings.json")
                            .UseInstaller<Bootstrapper>()
                            .BuildLongRunner();
            using (runner)
            {
                runner.RunAndWait();
            }

            LongRunningHarness.KeepWindowOpenWhenDebugging();
        }

        private static void SingleActionRunner(string[] args)
        {
            var runner = new RunnerBuilder()
                            .UseCommandLineArgs(args)
                            .UseConfig("appsettings.json")
                            .UseInstaller<Bootstrapper>()
                            .BuildSingleActionRunner();
            using (runner)
            {
                runner.Execute(args);
            }

            SingleActionHarness.KeepWindowOpenWhenDebugging();
        }

        private static void StartStopRunner(string[] args)
        {
            var runner = new RunnerBuilder()
                            .UseCommandLineArgs(args)
                            .UseConfig("appsettings.json")
                            .UseInstaller<Bootstrapper>()
                            .BuildStartStopRunner();
            using (runner)
            {
                runner.Start();

                Console.WriteLine("\r\n\r\nPress <Enter> to stop services...");
                Console.ReadLine();

                runner.Stop();
            }

            StartStopHarness.KeepWindowOpenWhenDebugging();
        }
    }
}
