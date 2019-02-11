namespace NFabric.Core.Recipes
{
    using System;

    /// <summary>
    /// Defines when the state in a <see cref="CircuitBreaker"/> should change.
    /// For example some exceptions might be severe and thus indicate quicker
    /// or even immediate change in the state.
    /// </summary>
    public interface ICircuitBreakerStrategy
    {
        void MarkSuccess();

        CircuitBreakerState Trip(Exception exception, CircuitBreakerState currentState);
    }
}
