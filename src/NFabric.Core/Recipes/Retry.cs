namespace NFabric.Core.Recipes
{
    using NFabric.Core.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public static class Retry
    {
        /// <summary>
        /// Strategy where you can define how many times it will re-try the action
        /// and if (and for how long) it will back-off between unsuccessful attempts.
        /// It is useful pattern when the action itself is quick, number of retries is small
        /// and also when back-off intervals are not very long.
        /// Therefore for cases when errors might happen but they are transient.
        /// </summary>
        public static async Task<T> ExecuteAsync<T>(
            Func<CancellationToken, Task<T>> retryable,
            IRetryIntervals intervals,
            CancellationToken ct = default(CancellationToken))
        {
            var failures = new List<RetryFailedAttempt>();
            TimeSpan interval;
            bool backOff;
            do
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    return await retryable(ct).ConfigureAwait(false);
                }
                catch (TaskCanceledException tcex)
                {
                    if (tcex.CancellationToken == ct)
                    {
                        // cancelled by caller (and not for example HttpClient timeout)
                        // https://stackoverflow.com/questions/10547895/how-can-i-tell-when-httpclient-has-timed-out
                        throw;
                    }

                    failures.Add(new RetryFailedAttempt(tcex));
                    interval = intervals.Next();
                    backOff = interval > TimeSpan.Zero;
                }
                catch (Exception ex)
                {
                    failures.Add(new RetryFailedAttempt(ex));
                    interval = intervals.Next();
                    backOff = interval > TimeSpan.Zero;
                }

                if (backOff)
                {
                    await Task.Delay(interval, ct).ConfigureAwait(false);
                }
            }
            while (backOff);

            throw new RetryException($"Action has failed despite retries (using {intervals}).", failures.ToArray());
        }

        public static async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> retryable, params int[] millis)
        {
            return await ExecuteAsync(retryable, new ExplicitRetryIntervals(millis)).ConfigureAwait(false);
        }

        /// <summary>
        /// Strategy where the action will be called until successful or deadline is reached (if any is defined).
        /// It is a 'holding at all costs' approach, so it should be reserved for cases
        /// where the action must succeed and it is worth to wait for it.
        /// Also, if the action itself might fail really quickly (without some timeout),
        /// do define back-off between retries; otherwise it might go into tight CPU loop.
        /// </summary>
        public static async Task<T> ExecuteUntilSuccessAsync<T>(
            Func<CancellationToken, Task<T>> retryable,
            TimeSpan? deadlineAfter = null,
            TimeSpan? backOffBetweenRetries = null,
            Type[] retryOnlyOnExceptions = null,
            CancellationToken ct = default(CancellationToken))
        {
            var pickyRetry = retryOnlyOnExceptions?.Length > 0;
            var start = DateTime.Now;
            var deadline = deadlineAfter.HasValue
                ? start.Add(deadlineAfter.Value)
                : DateTime.MaxValue;

            while (deadline > DateTime.Now)
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    return await retryable(ct).ConfigureAwait(false);
                }
                catch (TaskCanceledException tcex)
                {
                    if (tcex.CancellationToken == ct)
                    {
                        // cancelled by caller (and not for example HttpClient timeout)
                        // https://stackoverflow.com/questions/10547895/how-can-i-tell-when-httpclient-has-timed-out
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    if (pickyRetry && !retryOnlyOnExceptions.Any(t => ex.GetType().Implements(t)))
                    {
                        // the exception is not among specified types to be retried on
                        throw;
                    }
                }

                if (backOffBetweenRetries.HasValue)
                {
                    await Task.Delay(backOffBetweenRetries.Value, ct).ConfigureAwait(false);
                }
            }

            throw new RetryException("Action has failed despite retries. " +
                        $"It has reached the deadline {deadline} first. Retry process has started at {start}.");
        }
    }
}
