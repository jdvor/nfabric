namespace NFabric.Host.Conventions
{
    using App.Metrics;
    using Microsoft.Extensions.DependencyInjection;
    using NFabric.Core;
    using Microsoft.Extensions.Hosting;
    using Serilog;

    public static class DefaultMetrics
    {
        public static IMetricsRoot Create(MetricsOptions opts, AppInfo appInfo)
        {
            var builder = new MetricsBuilder()
                .Configuration
                    .Configure(options =>
                    {
                        options.Enabled = opts.Enabled;
                        options.ReportingEnabled = opts.ReportingEnabled;
                        options.GlobalTags = new GlobalMetricTags
                        {
                            { "app", appInfo.Name },
                            { "env", appInfo.Env },
                            { "host", appInfo.Host },
                        };
                    });

            if (opts.Enabled && opts.ReportingEnabled)
            {
                builder.Report
                    .ToInfluxDb(options =>
                    {
                        options.InfluxDb.BaseUri = opts.BaseUri;
                        options.InfluxDb.Database = opts.Database;
                        options.InfluxDb.CreateDataBaseIfNotExists = opts.CreateDataBaseIfNotExists;
                        options.HttpPolicy.BackoffPeriod = opts.BackoffInterval;
                        options.HttpPolicy.FailuresBeforeBackoff = opts.FailuresBeforeBackoff;
                        options.HttpPolicy.Timeout = opts.Timeout;
                        options.FlushInterval = opts.FlushInterval;
                    });
            }

            return builder.Build();
        }

        public static void ConfigureServices(IServiceCollection services, IMetricsRoot metrics, MetricsOptions options)
        {
            services.AddSingleton(metrics);
            services.AddSingleton<IMetrics>(metrics);

            if (options.Enabled)
            {
                services.AddSingleton<IHostedService>(provider =>
                {
                    var logger = provider.GetRequiredService<ILogger>();
                    return new MetricsPublisher(metrics, logger, options.FlushInterval);
                });
            }
        }
    }
}
