namespace NFabric.Core.Collections
{
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Queue will start to drop oldest items when new ones are added and the queue would grow over specified limit.
    /// It is simmilar to <see cref="CircularBuffer{T}"/>, but with queue semantic and thread safety.
    /// It is thread SAFE!
    /// </summary>
    [SuppressMessage(
        "Microsoft.Naming",
        "CA1710:IdentifiersShouldHaveCorrectSuffix",
        Justification = "The name is precise and descriptive as is.")]
    public sealed class ConcurrentQueueWithLimit<T> : ConcurrentQueue<T>
    {
        public ConcurrentQueueWithLimit(int limit)
        {
            Limit = limit;
        }

        private readonly object lck = new object();

        public int Limit { get; }

        [SuppressMessage(
            "Naming Rules",
            "SA1312:VariableNamesMustBeginWithLowerCaseLetter",
            Justification = "Name '_' conveys more important meaning of 'not relevant'.")]
        public new void Enqueue(T item)
        {
            base.Enqueue(item);
            lock (lck)
            {
                while (Count > Limit)
                {
                    T _;
                    TryDequeue(out _);
                }
            }
        }
    }
}
