namespace NFabric.Core.Recipes
{
    using System;

    /// <summary>
    /// Defines back-off intervals in re-try strategy.
    /// </summary>
    public interface IRetryIntervals
    {
        TimeSpan Next();
    }
}
