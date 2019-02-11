namespace NFabric.Core.Recipes
{
    using NFabric.Core;
    using NFabric.Core.Collections;
    using System;

    /// <summary>
    /// Circuit breaker strategy which counts errors over time and trips the circuit
    /// if such rate is too high (too many errors occuring too quickly).
    /// </summary>
    public sealed class MaxErrorsPerTimeStrategy : ICircuitBreakerStrategy
    {
        public MaxErrorsPerTimeStrategy(int maxErrorsInInterval, TimeSpan interval, Func<long> ticksFn = null)
        {
            Expect.Positive(maxErrorsInInterval, nameof(maxErrorsInInterval));

            errors = new CircularBuffer<long>(maxErrorsInInterval + 1);
            this.interval = interval.Ticks;
            this.maxErrorsInInterval = maxErrorsInInterval;
            this.ticksFn = ticksFn ?? Ticks;
        }

        private readonly long interval;
        private readonly int maxErrorsInInterval;
        private readonly Func<long> ticksFn;

        // CircularBuffer is not thread-safe
        private readonly CircularBuffer<long> errors;
        private readonly object wsync = new object();

        public void MarkSuccess()
        {
        }

        public CircuitBreakerState Trip(Exception exception, CircuitBreakerState currentState)
        {
            var now = ticksFn();
            var horizon = now - interval;
            int count;
            lock (wsync)
            {
                errors.Add(now);
                count = errors.CountItems(x => x > horizon);
            }

            if (count > maxErrorsInInterval)
            {
                return CircuitBreakerState.Open;
            }

            return count > 0 ? CircuitBreakerState.HalfOpen : CircuitBreakerState.Closed;
        }

        private static long Ticks() { return DateTime.UtcNow.Ticks; }
    }
}
