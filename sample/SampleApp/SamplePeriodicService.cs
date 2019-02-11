namespace SampleApp
{
    using App.Metrics;
    using NFabric.Core;
    using NFabric.Core.Recipes;
    using Serilog;
    using Serilog.Context;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class SamplePeriodicService : PeriodicBackgroundTask
    {
        private readonly ILogger logger;
        private readonly IMetrics metrics;
        private readonly Random rnd;

        public SamplePeriodicService(ILogger logger, IMetrics metrics, TimeSpan? interval = null)
            : base(interval ?? TimeSpan.FromSeconds(5))
        {
            Expect.NotNull(logger, nameof(logger));
            Expect.NotNull(metrics, nameof(metrics));
            this.logger = logger.ForContext<SamplePeriodicService>();
            this.metrics = metrics;
            rnd = new Random();
        }

        // default schedule is every 5 secs
        protected override async Task CreateTask(CancellationToken ct)
        {
            using (LogContext.PushProperty("BeatId", Guid.NewGuid()))
            {
                try
                {
                    using (Metric.SampleTimer(metrics))
                    {
                        // simulate some intensive work 150-600 ms, which will fail roughly in 5% of cases
                        await DoWork().ConfigureAwait(false);
                    }

                    Metric.Success(metrics);
                    logger.Information("Beat has succeeded.");
                }
                catch (Exception ex)
                {
                    Metric.Error(metrics);
                    logger.Error(ex, "Beat has failed.");
                }
            }
        }

        private async Task DoWork()
        {
            await Task.Delay(rnd.Next(150, 600)).ConfigureAwait(false);
            if (rnd.Next(1, 101) <= 5) // 5%
            {
                throw new ArithmeticException("intentional error thrown to test logging & metrics");
            }
        }
    }
}
