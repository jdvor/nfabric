namespace NFabric.Core.Recipes
{
    using System;

    public sealed class RetryFailedAttempt
    {
        public RetryFailedAttempt(DateTimeOffset time, Exception exception)
        {
            Time = time;
            Exception = exception;
        }

        public RetryFailedAttempt(Exception exception)
            : this(DateTimeOffset.Now, exception)
        {
        }

        public DateTimeOffset Time { get; }

        public Exception Exception { get; }
    }
}
