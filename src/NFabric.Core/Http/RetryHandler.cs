namespace NFabric.Core.Http
{
    using NFabric.Core.Recipes;
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class RetryHandler : DelegatingHandler
    {
        private readonly Func<IRetryIntervals> intervalsFactory;

        public RetryHandler(Func<IRetryIntervals> intervalsFactory)
        {
            this.intervalsFactory = intervalsFactory;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var intervals = intervalsFactory();
            return await Retry.ExecuteAsync(
                    async ct => await base.SendAsync(request, ct).ConfigureAwait(false),
                    intervals,
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
