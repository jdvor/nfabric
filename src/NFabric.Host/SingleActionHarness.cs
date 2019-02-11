namespace NFabric.Host
{
    using NFabric.Core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;
    using System;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// As opposed to <see cref="LongRunningHarness"/> this class is used to run short lived tasks based on name provided from elsewhere.
    /// Typically the name is passed as command line parameter to a console application.
    /// This class requires that in the <see cref="services"/>
    /// </summary>
    public sealed class SingleActionHarness : IDisposable
    {
        public SingleActionHarness(IServiceProvider services, AppInfo appInfo, ILogger logger)
        {
            this.services = services;
            this.appInfo = appInfo;
            this.RootLogger = logger.ForContext<LongRunningHarness>();
            cts = new CancellationTokenSource();
        }

        private readonly IServiceProvider services;
        private readonly AppInfo appInfo;
        private readonly CancellationTokenSource cts;
        private bool disposed;

        public ILogger RootLogger { get; }

        public void Execute(string[] args, string defaultActionName = null)
        {
            var actionName = args?.Length > 0 ? args[0] : defaultActionName;
            if (string.IsNullOrEmpty(actionName))
            {
                throw new ArgumentException("No action name has been provided.", nameof(args));
            }

            var actionFactory = services.GetRequiredService<Func<string, IAction>>();
            var action = actionFactory(actionName);
            if (action == null)
            {
                throw new NotImplementedException($"Action '{actionName}' has not implementation registered.");
            }

            var config = services.GetRequiredService<IConfiguration>();
            action.ExecuteAsync(config).GetAwaiter().GetResult();
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

            Log.CloseAndFlush();

            cts.Dispose();
            disposed = true;
        }
    }
}
