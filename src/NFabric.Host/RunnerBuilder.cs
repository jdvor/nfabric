using System.IO;
using System.Linq;

namespace NFabric.Host
{
    using App.Metrics;
    using NFabric.Core;
    using NFabric.Host.Conventions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;
    using System;
    using System.Collections.Generic;

    public sealed class RunnerBuilder
    {
        private string[] args;

        private readonly HashSet<string> configPaths = new HashSet<string>();

        private readonly Dictionary<string, IServiceInstaller> installers = new Dictionary<string, IServiceInstaller>();

        public RunnerBuilder UseCommandLineArgs(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                this.args = args;
            }

            return this;
        }

        public RunnerBuilder UseConfig(string configPath, bool optional = false)
        {
            if (optional == false)
            {
                Expect.FileExists(configPath, nameof(configPath));
            }

            if (File.Exists(configPath))
            {
                configPaths.Add(configPath);
            }

            return this;
        }

        public RunnerBuilder UseInstaller<T>()
            where T : IServiceInstaller, new()
        {
            UseInstaller(Activator.CreateInstance<T>());
            return this;
        }

        public RunnerBuilder UseInstaller(IServiceInstaller installer)
        {
            var key = installer.GetType().FullName;
            if (!installers.ContainsKey(key))
            {
                installers.Add(key, installer);
            }

            return this;
        }

        public RunnerBuilder UseInstallers(params IServiceInstaller[] installers)
        {
            foreach (var installer in installers)
            {
                UseInstaller(installer);
            }

            return this;
        }

        private (IServiceProvider, AppInfo, ILogger) BuildImpl()
        {
            var services = new ServiceCollection();

            // configuration
            var config = DefaultConfiguration.Create(configPaths.ToArray(), args);
            DefaultConfiguration.Configure(services, config);

            // app context
            var name = config.GetValue<string>("app:name", null);
            var env = config.GetValue<string>("app:env", null);
            var appInfo = new AppInfo(args, name, env);
            services.AddSingleton(appInfo);

            // logging
            var logsOptions = LogsOptions.Create(config, appInfo.Name);
            var (loggerFactory, defaultLogger) = DefaultLogging.Create(logsOptions, appInfo);
            DefaultLogging.ConfigureServices(services, loggerFactory, defaultLogger);

            // metrics
            var metricsOptions = Conventions.MetricsOptions.Create(config);
            IMetricsRoot metrics = DefaultMetrics.Create(metricsOptions, appInfo);
            DefaultMetrics.ConfigureServices(services, metrics, metricsOptions);

            // log few details about so far configured services for debugging purposes
            defaultLogger.Debug("StartUp: application {appInfo}", appInfo);
            defaultLogger.Debug("StartUp: logging {options}", logsOptions);
            defaultLogger.Debug("StartUp: metrics {options}", metricsOptions);

            // execute registered service installers
            foreach (var installer in installers.Values)
            {
                defaultLogger.Debug($"StartUp: service installer {installer.GetType().FullName}");
                installer.Install(services, config);
            }

            var provider = services.BuildServiceProvider(validateScopes: true);

            return (provider, appInfo, defaultLogger);
        }

        public LongRunningHarness BuildLongRunner()
        {
            var (provider, appInfo, defaultLogger) = BuildImpl();
            return new LongRunningHarness(provider, appInfo, defaultLogger);
        }

        public SingleActionHarness BuildSingleActionRunner()
        {
            var (provider, appInfo, defaultLogger) = BuildImpl();
            return new SingleActionHarness(provider, appInfo, defaultLogger);
        }

        public StartStopHarness BuildStartStopRunner()
        {
            var (provider, appInfo, defaultLogger) = BuildImpl();
            return new StartStopHarness(provider, appInfo, defaultLogger);
        }

        /// <summary>
        /// Metoda specialne pro WPF aplikace, ktere nepotrebuji zadny runner, ale hodi se logovani, metriky a konfigurace
        /// </summary>
        /// <returns></returns>
        public ServiceCollection Build()
        {
            var services = new ServiceCollection();

            // configuration
            var config = DefaultConfiguration.Create(configPaths.ToArray(), args);
            DefaultConfiguration.Configure(services, config);

            // app context
            var name = config.GetValue<string>("app:name", null);
            var env = config.GetValue<string>("app:env", null);
            var appInfo = new AppInfo(args, name, env);
            services.AddSingleton(appInfo);

            // logging
            var logsOptions = LogsOptions.Create(config, appInfo.Name);
            var (loggerFactory, defaultLogger) = DefaultLogging.Create(logsOptions, appInfo);
            DefaultLogging.ConfigureServices(services, loggerFactory, defaultLogger);

            // metrics
            var metricsOptions = Conventions.MetricsOptions.Create(config);
            IMetricsRoot metrics = DefaultMetrics.Create(metricsOptions, appInfo);
            DefaultMetrics.ConfigureServices(services, metrics, metricsOptions);

            // log few details about so far configured services for debugging purposes
            defaultLogger.Debug("StartUp: application {appInfo}", appInfo);
            defaultLogger.Debug("StartUp: logging {options}", logsOptions);
            defaultLogger.Debug("StartUp: metrics {options}", metricsOptions);

            return services;
        }
    }
}
