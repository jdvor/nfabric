namespace NFabric.Host
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using App.Metrics;
    using NFabric.Core;
    using NFabric.Core.Recipes;
    using Serilog;
    using Serilog.Context;

    /// <summary>
    /// Periodic publishing of application metrics into whatever is configured as reporter or reporters.
    /// The class might be superseeded by Microsoft.Extensions.Hosting.BackgroundService
    /// and respective changes in App.Metrics when .NET Core 2.1. is released.
    /// See https://www.app-metrics.io/reporting/reporters/console/.
    /// </summary>
    public sealed class MetricsPublisher : BackgroundTask
    {
        private readonly TimeSpan flushInterval;
        private readonly ILogger logger;
        private readonly IMetricsRoot metrics;

        public MetricsPublisher(IMetricsRoot metrics, ILogger logger, TimeSpan? flushInterval = null)
        {
            Expect.NotNull(metrics, nameof(metrics));
            Expect.NotNull(logger, nameof(logger));

            this.metrics = metrics;
            this.logger = logger.ForContext<MetricsPublisher>();
            this.flushInterval = flushInterval ?? TimeSpan.FromSeconds(10);
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            logger.Debug("Metrics publishing background task is starting.");
            ct.Register(() => logger.Debug("Metrics publishing background task is stopping."));

            while (!ct.IsCancellationRequested)
            {
                using (LogContext.PushProperty("PushId", Guid.NewGuid()))
                {
                    await PublishData(metrics, logger, ct).ConfigureAwait(false);
                }

                await Task.Delay(flushInterval, ct).ConfigureAwait(false);
            }

            // Signal to stop the background task have been recieved by token cancellation.
            // Run reporters one more time, so no metric data are lost.
            await PublishData(metrics, logger).ConfigureAwait(false);

            logger.Debug("Metrics publishing background task has finished.");
        }

        private static async Task PublishData(IMetricsRoot metrics, ILogger logger, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var tasks = metrics.ReportRunner.RunAllAsync(ct);
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Metrics reporters have failed to publish metrics data.");
            }
        }
    }
}
