namespace NFabric.Core.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class DictionaryExtensions
    {
        /// <summary>
        /// Consequently tries all keys and returns first value when key match is found.
        /// It compares keys using <see cref="StringComparer.OrdinalIgnoreCase"/>.
        /// </summary>
        public static T GetValueEagerly<T>(this IDictionary<string, T> dict, params string[] keys)
        {
            if (dict == null || keys?.Length == 0)
            {
                return default(T);
            }

            foreach (var key in dict.Keys)
            {
                if (keys.Contains(key, StringComparer.OrdinalIgnoreCase))
                {
                    return dict[key];
                }
            }

            return default(T);
        }
    }
}
