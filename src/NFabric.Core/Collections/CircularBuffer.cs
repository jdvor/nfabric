namespace NFabric.Core.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Buffer holding fixed number of items.
    /// Once full, the new items will start to override the oldest ones.
    /// NOT thread-safe!
    /// </summary>
    [SuppressMessage(
        "Microsoft.Naming",
        "CA1710:IdentifiersShouldHaveCorrectSuffix",
        Justification = "The name is precise and descriptive as is.")]
    public sealed class CircularBuffer<T> : IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>
        where T : IEquatable<T>
    {
        public CircularBuffer(int capacity)
        {
            buffer = new T[capacity];
            lastIdx = capacity - 1;
            position = -1;
            isFull = false;
        }

        private readonly T[] buffer;
        private readonly int lastIdx;
        private int position;
        private bool isFull;

        public void Add(T item)
        {
            buffer[++position] = item;
            if (position == lastIdx)
            {
                isFull = true;
                position = -1;
            }
        }

        public bool TryGet(Func<T, bool> predicate, out T item)
        {
            if (isFull)
            {
                foreach (var e in buffer)
                {
                    if (predicate(e))
                    {
                        item = e;
                        return true;
                    }
                }
            }
            else if (position > -1)
            {
                for (var i = 0; i <= position; i++)
                {
                    var e = buffer[i];
                    if (predicate(e))
                    {
                        item = e;
                        return true;
                    }
                }
            }

            item = default(T);
            return false;
        }

        public bool Contains(T item)
        {
            if (isFull)
            {
                foreach (var e in buffer)
                {
                    if (e.Equals(item))
                    {
                        return true;
                    }
                }
            }
            else if (position > -1)
            {
                for (var i = 0; i <= position; i++)
                {
                    var e = buffer[i];
                    if (e.Equals(item))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public T[] Find(Func<T, bool> predicate)
        {
            if (isFull)
            {
                return buffer.Where(x => predicate(x)).ToArray();
            }
            else if (position > -1)
            {
                return new ArraySegment<T>(buffer, 0, position + 1).Where(x => predicate(x)).ToArray();
            }

            return Array.Empty<T>();
        }

        public int CountItems(Func<T, bool> predicate)
        {
            if (isFull)
            {
                return buffer.Count(x => predicate(x));
            }
            else if (position > -1)
            {
                return new ArraySegment<T>(buffer, 0, position + 1).Count(x => predicate(x));
            }

            return 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (isFull)
            {
                if (position == -1)
                {
                    for (var i = 0; i <= lastIdx; i++)
                    {
                        yield return buffer[i];
                    }
                }
                else
                {
                    for (var i = position; i <= lastIdx; i++)
                    {
                        yield return buffer[i];
                    }

                    for (var i = 0; i < position; i++)
                    {
                        yield return buffer[i];
                    }
                }
            }
            else if (position > -1)
            {
                for (var i = 0; i <= position; i++)
                {
                    yield return buffer[i];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            var sb = new StringBuilder($"{nameof(CircularBuffer<T>)} ");
            var e = GetEnumerator();
            var first = true;
            var len = Count;
            if (len == 0)
            {
                sb.Append("empty");
            }
            else if (len <= 100)
            {
                sb.Append("[");
                while (e.MoveNext())
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sb.Append(", ");
                    }

                    sb.Append(e.Current.ToString());
                }

                sb.Append("]");
            }
            else
            {
                sb.AppendFormat("{0} long", len);
            }

            return sb.ToString();
        }

        public bool IsEmpty => !(isFull || position > -1);

        public int Count => isFull ? lastIdx + 1 : position + 1;
    }
}
