namespace NFabric.Core.Extensions
{
    using System;
    using System.Collections.Concurrent;

    public static class FuncExtensions
    {
        /// <summary>
        /// It caches result of a function until the end of the process / application / domain.
        /// The function must be <see cref="https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/refactoring-into-pure-functions">pure</see>.
        /// </summary>
        /// <param name="f">Function to extend with cache. It must be pure.</param>
        public static Func<TArg1, TResult> Cached<TArg1, TResult>(this Func<TArg1, TResult> f)
        {
            var cache = new ConcurrentDictionary<TArg1, TResult>();
            return arg1 =>
            {
                if (cache.TryGetValue(arg1, out TResult value))
                {
                    return value;
                }

                value = f(arg1);
                cache.TryAdd(arg1, value);

                return value;
            };
        }

        /// <summary>
        /// It caches result of a function until the end of the process / application / domain.
        /// The function must be <see cref="https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/refactoring-into-pure-functions">pure</see>.
        /// </summary>
        /// <param name="f">Function to extend with cache. It must be pure.</param>
        public static Func<TArg1, TArg2, TResult> Cached<TArg1, TArg2, TResult>(this Func<TArg1, TArg2, TResult> f)
        {
            var cache = new ConcurrentDictionary<Tuple<TArg1, TArg2>, TResult>();
            return (arg1, arg2) =>
            {
                var key = new Tuple<TArg1, TArg2>(arg1, arg2);
                if (cache.TryGetValue(key, out TResult value))
                {
                    return value;
                }

                value = f(arg1, arg2);
                cache.TryAdd(key, value);

                return value;
            };
        }

        /// <summary>
        /// It caches result of a function until the end of the process / application / domain.
        /// The function must be <see cref="https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/refactoring-into-pure-functions">pure</see>.
        /// </summary>
        /// <param name="f">Function to extend with cache. It must be pure.</param>
        public static Func<TArg1, TArg2, TArg3, TResult> Cached<TArg1, TArg2, TArg3, TResult>(this Func<TArg1, TArg2, TArg3, TResult> f)
        {
            var cache = new ConcurrentDictionary<Tuple<TArg1, TArg2, TArg3>, TResult>();
            return (arg1, arg2, arg3) =>
            {
                var key = new Tuple<TArg1, TArg2, TArg3>(arg1, arg2, arg3);
                if (cache.TryGetValue(key, out TResult value))
                {
                    return value;
                }

                value = f(arg1, arg2, arg3);
                cache.TryAdd(key, value);

                return value;
            };
        }

        /// <summary>
        /// It caches result of a function until the end of the process / application / domain.
        /// The function must be <see cref="https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/refactoring-into-pure-functions">pure</see>.
        /// </summary>
        /// <param name="f">Function to extend with cache. It must be pure.</param>
        public static Func<TArg1, TArg2, TArg3, TArg4, TResult> Cached<TArg1, TArg2, TArg3, TArg4, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, TResult> f)
        {
            var cache = new ConcurrentDictionary<Tuple<TArg1, TArg2, TArg3, TArg4>, TResult>();
            return (arg1, arg2, arg3, arg4) =>
            {
                var key = new Tuple<TArg1, TArg2, TArg3, TArg4>(arg1, arg2, arg3, arg4);
                if (cache.TryGetValue(key, out TResult value))
                {
                    return value;
                }

                value = f(arg1, arg2, arg3, arg4);
                cache.TryAdd(key, value);

                return value;
            };
        }

        /// <summary>
        /// It caches result of a function until the end of the process / application / domain.
        /// The function must be <see cref="https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/refactoring-into-pure-functions">pure</see>.
        /// </summary>
        /// <param name="f">Function to extend with cache. It must be pure.</param>
        public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TResult> Cached<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, TArg5, TResult> f)
        {
            var cache = new ConcurrentDictionary<Tuple<TArg1, TArg2, TArg3, TArg4, TArg5>, TResult>();
            return (arg1, arg2, arg3, arg4, arg5) =>
            {
                var key = new Tuple<TArg1, TArg2, TArg3, TArg4, TArg5>(arg1, arg2, arg3, arg4, arg5);
                if (cache.TryGetValue(key, out TResult value))
                {
                    return value;
                }

                value = f(arg1, arg2, arg3, arg4, arg5);
                cache.TryAdd(key, value);

                return value;
            };
        }
    }
}
