namespace NFabric.Core.Recipes
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Base class for back-ground tasks which are executed periodically every X seconds.
    /// </summary>
    public abstract class PeriodicBackgroundTask : BackgroundTask
    {
        private TimeSpan interval;

        protected PeriodicBackgroundTask(TimeSpan interval)
        {
            this.interval = interval;
        }

        /// <summary>
        /// Returned task will be awaited at every interval, so it is a factory method.
        /// </summary>
        protected abstract Task CreateTask(CancellationToken ct);

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var task = CreateTask(ct);
                await task.ConfigureAwait(false);
                await Task.Delay(interval, ct).ConfigureAwait(false);
            }
        }
    }
}
