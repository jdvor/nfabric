namespace NFabric.Core.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Extension methods for <see cref="Type"/> type.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Returns <c>true</c> if <paramref name="givenType"/> implements <paramref name="testType"/>.
        /// </summary>
        public static bool Implements(this Type givenType, Type testType)
        {
            if (givenType == null || testType == null)
            {
                return false;
            }

            return testType.GetTypeInfo().IsAssignableFrom(givenType);
        }

        /// <summary>
        /// Returns <c>true</c> if <paramref name="givenType"/> implements any of <paramref name="testTypes"/>.
        /// </summary>
        public static bool ImplementsAnyOf(this Type givenType, ICollection<Type> testTypes)
        {
            if (givenType == null || testTypes?.Count == 0)
            {
                return false;
            }

            return testTypes.Any(t => t.GetTypeInfo().IsAssignableFrom(givenType));
        }

        /// <summary>
        /// Returns <c>true</c> if generic <paramref name="givenType"/> implements generic <paramref name="testType"/>.
        /// </summary>
        public static bool ImplementsGeneric(this Type givenType, Type genericType)
        {
            if (givenType == null || genericType == null)
            {
                return false;
            }

            return givenType == genericType
                   || givenType.MapsToGenericTypeDefinition(genericType)
                   || givenType.HasInterfaceThatMapsToGenericTypeDefinition(genericType)
                   || givenType.GetTypeInfo().BaseType.ImplementsGeneric(genericType);
        }

        /// <summary>
        /// Returns type of generic argument of <paramref name="type"/> at position <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Zero-based position of generic type arguments. First is 0, second is 1, etc.</param>
        /// <example>
        ///     var genericType = new Dictionary&lt;string, int&gt;();
        ///     var t1 = genericType.GenericArgument(1); // t1 is System.Int32
        /// </example>
        public static Type GenericArgument(this Type type, int index = 0)
        {
            if (type == null || !type.GetTypeInfo().IsGenericType)
            {
                return null;
            }

            var args = type.GetTypeInfo().GetGenericArguments();
            return index >= args.Length
                ? null
                : args[index];
        }

        /// <summary>
        /// Gets qualified name for type including assembly name, but without assembly version, culture, etc.
        /// </summary>
        public static string GetRelaxedFullName(this Type type)
        {
            var ns = type.Namespace != null ? $"{type.Namespace}." : string.Empty;
            return $"{ns}{type.Name}, {type.GetTypeInfo().Assembly.GetName().Name}";
        }

        public static bool IsNumber(this Type type)
        {
            var typeCode = Type.GetTypeCode(type);
            return typeCode == TypeCode.Decimal
                    || (type.IsPrimitive && typeCode != TypeCode.Object && typeCode != TypeCode.Boolean && typeCode != TypeCode.Char);
        }

        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HasInterfaceThatMapsToGenericTypeDefinition(this Type givenType, Type genericType)
        {
            return givenType
                .GetTypeInfo().GetInterfaces()
                .Where(it => it.GetTypeInfo().IsGenericType)
                .Any(it => it.GetGenericTypeDefinition() == genericType);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool MapsToGenericTypeDefinition(this Type givenType, Type genericType)
        {
            return genericType.GetTypeInfo().IsGenericTypeDefinition
                   && givenType.GetTypeInfo().IsGenericType
                   && givenType.GetGenericTypeDefinition() == genericType;
        }
    }
}
