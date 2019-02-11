namespace NFabric.Core.Recipes
{
    using System;

    /// <summary>
    /// https://particular.net/blog/protect-your-software-with-the-circuit-breaker-design-pattern
    /// </summary>
    public class CircuitBreakerOpenException : Exception
    {
        public CircuitBreakerOpenException(Exception ex, string name)
            : base($"circuit breaker {name} is open", ex)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
