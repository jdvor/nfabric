namespace NFabric.Core.Extensions
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    /// <summary>
    /// Extension methods for <see cref="IEnumerable{T}"/> type.
    /// </summary>
    public static class EnumerableExtensions
    {
        public static IEnumerable<TResult> SelectNonNull<T, TResult>(this IEnumerable<T> sequence, Func<T, TResult> projection)
        {
            return sequence.Select(projection).Where(x => x != null);
        }
    }
}
