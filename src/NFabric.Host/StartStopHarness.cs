using NFabric.Core;
using NFabric.Core.Extensions;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NFabric.Host
{
    public sealed class StartStopHarness : IDisposable
    {
        private readonly IServiceProvider services;
        private readonly AppInfo appInfo;
        private readonly CancellationTokenSource cts;
        private IHostedService[] hosted;
        private bool disposed;

        public StartStopHarness([NotNull] IServiceProvider services, [NotNull] AppInfo appInfo, [NotNull] ILogger logger)
        {
            this.services = services;
            this.appInfo = appInfo;
            this.RootLogger = logger;
            cts = new CancellationTokenSource();
        }

        public ILogger RootLogger { get; }

        public void Start()
        {
            if (!StartUp())
            {
                Dispose();
            }
        }

        private bool StartUp()
        {
            hosted = services.GetServices<IHostedService>().ToArray();
            if (hosted.Length == 0)
            {
                RootLogger.Error("No services (implementations of IHostedService) have been discovered.");
                return false;
            }

            RootLogger.Debug($"Discovered services: {string.Join(", ", hosted.Select(x => x.GetType().Name))}");

            var ct = cts.Token;
            var startupTasks = hosted.Select(x => x.StartAsync(ct)).ToArray();
            Task.WaitAll(startupTasks);

            var ok = startupTasks.HasAllSuccessfulyCompleted();
            if (!ok)
            {
                RootLogger.ExceptionsAsError(startupTasks, "Service has failed to start successfuly.");
            }

            return ok;
        }

        public void Stop()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            Shutdown();
            hosted = Array.Empty<IHostedService>();

            disposed = true;
        }

        private void Shutdown()
        {
            RootLogger.Debug($"Application {appInfo.Name} is going to exit.");

            // cts is cancellation token source which have been used to inject tokens into startup StartAsync methods of hosted services.
            cts.Cancel();

            var stopTasks = hosted.Select(x => x.StopAsync(default(CancellationToken))).ToArray();

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
    }
}
