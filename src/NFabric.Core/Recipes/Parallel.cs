namespace NFabric.Core.Recipes
{
    using NFabric.Core.Extensions;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Controls execution of a set of asynchronuos tasks, when there is a limit to execution paralellism.
    /// Simillar to <see cref="Parallel.ForEach"/>, but for awaitable tasks.
    /// </summary>
    public static class Parallel
    {
        public static async Task ForEachAsync<T>(
            IEnumerable<T> source,
            Func<T, CancellationToken, Task> body,
            int parallelism = 0,
            CancellationToken cancelTkn = default(CancellationToken))
        {
            Expect.NotNull(source, nameof(source));
            Expect.NotNull(body, nameof(body));

            var partitionCount = parallelism > 0 ? parallelism : Environment.ProcessorCount;
            var tasks = Partitioner.Create(source)
                .GetPartitions(partitionCount)
                .Select(async p =>
                {
                    using (p)
                    {
                        while (p.MoveNext())
                        {
                            cancelTkn.ThrowIfCancellationRequested();
                            await body(p.Current, cancelTkn).ConfigureAwait(false);
                        }
                    }
                });

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Controls execution of a set of asynchronuos tasks, when there is a limit to execution paralellism.
        /// Simillar to <see cref="Parallel.ForEach"/>, but for awaitable tasks.
        /// </summary>
        public static async Task<IEnumerable<TResult>> ForEachAsync<TSource, TResult>(
            IEnumerable<TSource> source,
            Func<TSource, CancellationToken, Task<TResult>> body,
            int parallelism = 0,
            CancellationToken cancelTkn = default(CancellationToken))
        {
            Expect.NotNull(source, nameof(source));
            Expect.NotNull(body, nameof(body));

            var partitionCount = parallelism > 0 ? parallelism : Environment.ProcessorCount;
            var tasks = Partitioner.Create(source)
                .GetPartitions(partitionCount)
                .Select(async p =>
                {
                    var partialResults = new List<TResult>();
                    using (p)
                    {
                        while (p.MoveNext())
                        {
                            cancelTkn.ThrowIfCancellationRequested();
                            var result = await body(p.Current, cancelTkn).ConfigureAwait(false);
                            partialResults.Add(result);
                        }
                    }

                    return partialResults;
                });

            await Task.WhenAll(tasks).ConfigureAwait(false);

            return tasks.ExtractResults().SelectMany(x => x);
        }
    }
}
