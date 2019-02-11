namespace NFabric.Core.Recipes
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Base class for 'components' with start & stop semantics.
    /// The class might be superseeded by Microsoft.Extensions.Hosting.BackgroundService when .NET Core 2.1. is released.
    /// </summary>
    public abstract class BackgroundTask : IHostedService, IDisposable
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        private Task executing;

        private bool disposed;

        protected abstract Task ExecuteAsync(CancellationToken ct);

        /// <summary>
        /// Called at the start-up of a process.
        /// </summary>
        public Task StartAsync(CancellationToken ct)
        {
            executing = ExecuteAsync(cts.Token);
            if (executing.IsCompleted)
            {
                // if the task is completed then return it, will bubble cancellation and failure to the caller
                return executing;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Called at the end of a process to signal
        /// that whatever is going on in the background task should be gracefully shut down.
        /// </summary>
        public async Task StopAsync(CancellationToken ct)
        {
            if (executing == null)
            {
                return;
            }

            try
            {
                // signal cancellation
                cts.Cancel();
            }
            finally
            {
                // either internal task finishes or cancelation signal (ct) terminates inifite delay
                await Task.WhenAny(executing, Task.Delay(Timeout.Infinite, ct)).ConfigureAwait(false);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            try
            {
                if (disposing)
                {
                    cts.Cancel();
                }
            }
            finally
            {
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
