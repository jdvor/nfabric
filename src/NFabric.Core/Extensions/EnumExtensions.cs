namespace NFabric.Core.Extensions
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;

    public static class EnumExtensions
    {
        private static readonly ConcurrentDictionary<Type, Dictionary<string, int>> Maps = new ConcurrentDictionary<Type, Dictionary<string, int>>();

        public static bool TryParseAsEnum<T>(this string s, out T value)
            where T : struct
        {
            if (string.IsNullOrEmpty(s))
            {
                value = default(T);
                return false;
            }

            var type = typeof(T);
            if (!Maps.TryGetValue(type, out Dictionary<string, int> map))
            {
                map = CreateMap<T>();
                Maps.TryAdd(type, map);
            }

            if (map.TryGetValue(s, out int valueAsInt))
            {
                value = (T)Enum.ToObject(type, valueAsInt);
                return true;
            }

            value = default(T);
            return false;
        }

        private static Dictionary<string, int> CreateMap<T>()
            where T : struct
        {
            var mems = GetEnumMemberAttributes<T, EnumMemberAttribute>();
            var result = new Dictionary<string, int>(mems.Count, StringComparer.Ordinal);
            foreach (var (value, name, attrs) in mems)
            {
                if (attrs.Length != 1) // EnumMemberAttribute has AllowMultiple = false
                {
                    continue;
                }

                var valueAsInt = Convert.ToInt32(value);
                result.Add(name, valueAsInt);
                result.Add(attrs[0].Value, valueAsInt);
            }

            return result;
        }

        public static ICollection<(TEnum MemberValue, string MemberName, TAttr[] Attribute)> GetEnumMemberAttributes<TEnum, TAttr>()
            where TEnum : struct
            where TAttr : Attribute
        {
            var type = typeof(TEnum);
            var values = Enum.GetValues(type);
            var result = new List<(TEnum MemberValue, string MemberName, TAttr[] Attribute)>(values.Length);
            foreach (var value in values)
            {
                var name = Enum.GetName(type, value);
                var memInfo = type.GetMember(name)[0];
                var attrs = memInfo.GetCustomAttributes(typeof(TAttr), false).Cast<TAttr>().ToArray();
                result.Add((MemberValue: (TEnum)value, MemberName: name, Attribute: attrs));
            }

            return result;
        }
    }
}
