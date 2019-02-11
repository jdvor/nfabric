namespace NFabric.Core.Recipes
{
    using System;

    public class RetryException : Exception
    {
        public RetryException(string message, RetryFailedAttempt[] failures = null)
            : base(message)
        {
            Failures = failures ?? Array.Empty<RetryFailedAttempt>();
        }

        public RetryFailedAttempt[] Failures { get; }
    }
}
