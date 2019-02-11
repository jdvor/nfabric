namespace NFabric.Core.Recipes
{
    using System;
    using static CircuitBreakerState;

    /// <summary>
    /// https://particular.net/blog/protect-your-software-with-the-circuit-breaker-design-pattern
    /// </summary>
    /// <exception cref="CircuitBreakerOpenException" />
    /// <example>
    /// var strategy = new MaxErrorsPerTimeStrategy(10, TimeSpan.FromMinutes(1));
    /// var breaker = new CircuitBreaker(strategy, "PreciousResource1");
    /// try
    /// {
    ///     breaker.ExecuteAction(() =>
    ///     {
    ///         // Operation protected by the circuit breaker.
    ///         ...
    ///     });
    /// }
    /// catch (CircuitBreakerOpenException ex)
    /// {
    ///     // Perform some different action when the breaker is open.
    ///     // Last exception details are in the inner exception and so is the name of the circuit breaker.
    ///     ...
    /// }
    /// catch (Exception ex)
    /// {
    ///     ...
    /// }
    /// </example>
    public sealed class CircuitBreaker
    {
        public CircuitBreaker(ICircuitBreakerStrategy strategy, string name)
        {
            this.strategy = strategy;
            Name = name;
        }

        private readonly ICircuitBreakerStrategy strategy;
        private readonly object stateSync = new object();

        public string Name { get; }

        public CircuitBreakerState State { get; private set; } = Closed;

        public Exception LastException { get; private set; }

        public DateTimeOffset? LastStateChanged { get; private set; }

        public bool IsOpen => State != Closed;

        public bool IsClosed => State == Closed;

        public void Execute(Action action)
        {
            if (IsOpen)
            {
                throw new CircuitBreakerOpenException(LastException, Name);
            }

            try
            {
                action();
                strategy.MarkSuccess();
            }
            catch (Exception ex)
            {
                LastException = ex;

                lock (stateSync)
                {
                    State = strategy.Trip(ex, State);
                }

                throw;
            }
        }
    }
}
