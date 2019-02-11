namespace NFabric.Host
{
    using System;
    using NFabric.Core;
    using NFabric.Core.Extensions;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using System.Diagnostics;
    using Serilog;
    using JetBrains.Annotations;

    public sealed class LongRunningHarness : IDisposable
    {
        public LongRunningHarness([NotNull] IServiceProvider services, [NotNull] AppInfo appInfo, [NotNull] ILogger logger)
        {
            this.services = services;
            this.appInfo = appInfo;
            this.RootLogger = logger.ForContext<LongRunningHarness>();
            exitSignal = new ManualResetEvent(false);
            cts = new CancellationTokenSource();
        }

        private readonly IServiceProvider services;
        private readonly AppInfo appInfo;
        private readonly ManualResetEvent exitSignal;
        private readonly CancellationTokenSource cts;
        private bool disposed;

        public ILogger RootLogger { get; }

        public void RunAndWait()
        {
            // SIGTERM (sent by systemd upon daemon termination)
            AppDomain.CurrentDomain.ProcessExit += OnAppExit;

            // SIGINT (Ctrl+C)
            Console.CancelKeyPress += OnAppExit;

            if (StartUp())
            {
                RootLogger.Information($"Application {appInfo.Name} is running and waiting for termination signal.");
                exitSignal.WaitOne();
            }
        }

        private bool StartUp()
        {
            var startupServices = services.GetServices<IHostedService>();

            RootLogger.Debug($"Discovered services: {string.Join(", ", startupServices.Select(x => x.GetType().Name))}");

            var ct = cts.Token;
            var startupTasks = startupServices.Select(x => x.StartAsync(ct)).ToArray();
            Task.WaitAll(startupTasks);

            var ok = startupTasks.HasAllSuccessfulyCompleted();
            if (!ok)
            {
                RootLogger.ExceptionsAsError(startupTasks, "Service has failed to start successfuly.");
            }

            return ok;
        }

        private void OnAppExit(object sender, EventArgs e)
        {
            RootLogger.Debug($"Termination signal {e.GetType().Name} has been recieved.");
            switch (e)
            {
                case ConsoleCancelEventArgs cc:
                    cc.Cancel = true;
                    break;
            }

            exitSignal.Set();
        }

        private void Shutdown()
        {
            RootLogger.Debug($"Application {appInfo.Name} is going to exit.");

            cts.Cancel();

            var stopTasks = services
                .GetServices<IHostedService>()
                .Select(x => x.StopAsync(default(CancellationToken)))
                .ToArray();

            Task.WaitAll(stopTasks);
            if (!stopTasks.HasAllSuccessfulyCompleted())
            {
                RootLogger.ExceptionsAsError(stopTasks, "Service has failed to shut down gracefully.");
            }

            RootLogger.Information($"Application {appInfo.Name} has terminated.");

            // important to call as most log sinks have buffering on
            Log.CloseAndFlush();
        }

        [Conditional("DEBUG")]
        public static void KeepWindowOpenWhenDebugging()
        {
            if (Debugger.IsAttached)
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to close the window...");
                Console.ReadKey();
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            Shutdown();

            cts.Dispose();
            exitSignal.Dispose();
            disposed = true;
        }
    }
}
