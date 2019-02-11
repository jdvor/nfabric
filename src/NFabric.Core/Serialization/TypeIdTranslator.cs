namespace NFabric.Core.Serialization
{
    using NFabric.Core.Extensions;
    using JetBrains.Annotations;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    public sealed class TypeIdTranslator
    {
        private readonly Dictionary<Type, string> type2Ids = new Dictionary<Type, string>();
        private readonly Dictionary<string, Type> ids2Type = new Dictionary<string, Type>();
        private readonly object sync = new object();

        public TypeIdTranslator([NotNull] IEnumerable<Type> initializeTypes)
        {
            if (initializeTypes == null)
            {
                return;
            }

            foreach (var type in initializeTypes)
            {
                var ids = CreateShortTypeIds(type);
                type2Ids.Add(type, ids[0]);
                foreach (var id in ids)
                {
                    ids2Type.Add(id, type);
                }
            }
        }

        public string Translate(Type type)
        {
            string id;
            if (type2Ids.TryGetValue(type, out id))
            {
                return id;
            }

            var ids = CreateShortTypeIds(type);
            lock (sync)
            {
                id = ids[0];
                if (!ids2Type.ContainsKey(id))
                {
                    type2Ids.Add(type, ids[0]);
                    foreach (var id2 in ids)
                    {
                        ids2Type.Add(id2, type);
                    }
                }
            }

            return id;
        }

        public Type Translate(string typeId)
        {
            if (ids2Type.TryGetValue(typeId, out Type t))
            {
                return t;
            }

            throw new SerializationException($"short ID {typeId} is not mapped to any type");
        }

        private static string[] CreateShortTypeIds(Type type)
        {
            var ids = new List<string>();

            var dn = type.GetCustomAttribute<DisplayNameAttribute>();
            if (dn != null)
            {
                ids.Add(dn.DisplayName);
            }

            var tag = CreateTag(type);
            ids.Add(tag);

            return ids.Distinct().ToArray();
        }

        private static string CreateTag(Type type)
        {
            var tag = CreateTagForBasicType(type);
            if (tag != null)
            {
                return tag;
            }

            if (type.IsArray)
            {
                var eType = type.GetElementType();
                return $"{CreateTag(eType)}[]";
            }

            if (type.IsConstructedGenericType)
            {
                var rawBaseTag = type.Name.SubstrUntilFirst('`');
                var baseTag = SimplifyBaseTag(rawBaseTag) ?? $"{rawBaseTag}.{TypeHash(type)}";
                var genArgTypes = type.GetGenericArguments();
                var genArgTags = string.Join("|", genArgTypes.Select(CreateTag));
                return $"{baseTag}({genArgTags})";
            }

            tag = new string(type.Name.Where(char.IsUpper).ToArray());
            return $"{tag}.{TypeHash(type)}";
        }

        private static string CreateTagForBasicType(Type type)
        {
            var nType = Nullable.GetUnderlyingType(type);
            var isNullable = nType != null;
            var currType = isNullable ? nType : type;

            if (currType == typeof(string))
            {
                return isNullable ? "str?" : "str";
            }

            if (currType == typeof(int))
            {
                return isNullable ? "int?" : "int";
            }

            if (currType == typeof(long))
            {
                return isNullable ? "long?" : "long";
            }

            if (currType == typeof(double))
            {
                return isNullable ? "d?" : "d";
            }

            if (currType == typeof(decimal))
            {
                return isNullable ? "m?" : "m";
            }

            if (currType == typeof(float))
            {
                return isNullable ? "f?" : "f";
            }

            if (currType == typeof(bool))
            {
                return isNullable ? "bool?" : "bool";
            }

            if (currType == typeof(byte))
            {
                return isNullable ? "byte?" : "byte";
            }

            if (currType == typeof(Guid))
            {
                return isNullable ? "guid?" : "guid";
            }

            if (currType == typeof(DateTimeOffset))
            {
                return isNullable ? "dto?" : "dto";
            }

            if (currType == typeof(DateTime))
            {
                return isNullable ? "dt?" : "dt";
            }

            if (currType == typeof(TimeSpan))
            {
                return isNullable ? "ts?" : "ts";
            }

            if (currType == typeof(object))
            {
                return "obj";
            }

            return null;
        }

        private static string SimplifyBaseTag(string baseTag)
        {
            switch (baseTag)
            {
                case "Dictionary":
                case "IDictionary":
                    return "dict";

                case "List":
                case "IList":
                    return "list";

                case "ICollection":
                    return "col";
            }

            return null;
        }

        private static string TypeHash(Type type)
        {
            var hashUint32 = MurmurHash2.Hash(Encoding.ASCII.GetBytes(type.FullName));
            var hashBytes = BitConverter.GetBytes(hashUint32);
            return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
        }
    }
}
