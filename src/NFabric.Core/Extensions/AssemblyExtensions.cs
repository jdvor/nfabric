namespace NFabric.Core.Extensions
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Extension methods for <see cref="Assembly"/> type.
    /// </summary>
    public static class AssemblyExtensions
    {
        public static Type[] FindTypes(this Assembly assembly, Func<Type, bool> predicate)
        {
            return assembly.GetExportedTypes().Where(predicate).ToArray();
        }

        public static Type[] FindImplementationsOf(this Assembly assembly, Type type, string namespaceStartsWith = null)
        {
            return FindTypes(assembly, t =>
            {
                var ti = t.GetTypeInfo();
                var ns = t.Namespace ?? string.Empty;
                return ti.IsClass &&
                       !ti.IsAbstract &&
                       !ti.IsInterface &&
                       (namespaceStartsWith == null || ns.StartsWith(namespaceStartsWith)) &&
                       t.Implements(type);
            });
        }

        public static string GetVersion(this Assembly assembly)
        {
            var info = FileVersionInfo.GetVersionInfo(assembly.Location);
            return info.FileVersion ?? assembly.GetName().Version.ToString();
        }

        public static byte[] Embedded(this Assembly assembly, string name, bool throwOnError = false)
        {
            try
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                using (var stream = assembly.GetManifestResourceStream(name))
                using (var reader = new BinaryReader(stream, Encoding.ASCII))
                {
                    // ReSharper disable once PossibleNullReferenceException
                    return reader.ReadBytes((int)stream.Length);
                }
            }
            catch
            {
                if (throwOnError)
                {
                    throw;
                }

                return null;
            }
        }
    }
}
