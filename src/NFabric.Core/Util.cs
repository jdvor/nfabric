namespace NFabric.Core
{
    using NFabric.Core.Extensions;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Reflection;

    /// <summary>
    /// Various helper methods for which more sensible home have not been found.
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// https://stackoverflow.com/questions/804700/how-to-find-fqdn-of-local-machine-in-c-net#804719
        /// </summary>
        public static string GetHostFqdn()
        {
            string fqdn;
            var ipProps = IPGlobalProperties.GetIPGlobalProperties();
            try
            {
                var domainSufix = "." + ipProps.DomainName;
                var hostName = Dns.GetHostName();
                fqdn = hostName.EndsWith(domainSufix)
                    ? hostName
                    : hostName + domainSufix;
            }
            catch (SocketException ex)
            {
                // DNS request failure => fallback to NETBIOS name; might work identical in some cases
                Debug.WriteLine($"error getting host FQDN via DNS - {ex.GetType().Name}: {ex.Message}");
                fqdn = $"{ipProps.DomainName}.{ipProps.HostName}";
            }

            return fqdn.ToLowerInvariant();
        }

        /// <summary>
        /// Extracts simple name for FQDN. For example: 'gandr' in 'gandr.nanoener.local'.
        /// </summary>
        /// <param name="fqdn">fully qualified domain name</param>
        public static string SimpleHostNameFromFqdn(string fqdn)
        {
            int idx;
            if (!string.IsNullOrEmpty(fqdn) && (idx = fqdn.IndexOf('.')) > 0)
            {
                return fqdn.Substring(0, idx).ToLowerInvariant();
            }

            return fqdn;
        }

        /// <summary>
        /// Tries to guess application name from its path while ignoring directories such as 'bin', 'Release', etc.
        /// </summary>
        public static string GuessAppNameFromPath()
        {
            var parts = AppContext.BaseDirectory
                .Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries)
                .Reverse();
            foreach (var part in parts)
            {
                if (!IgnoreNames.Contains(part, StringComparer.InvariantCultureIgnoreCase))
                {
                    return part.PascalCase();
                }
            }

            return null;
        }

        /// <summary>
        /// Looks for application version in assembly metadata.
        /// </summary>
        /// <returns>semantic application version</returns>
        public static string GetAppVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return $"{fvi.ProductMajorPart}.{fvi.ProductMinorPart}.{fvi.ProductBuildPart}.{fvi.ProductPrivatePart}";
        }

        public static IServiceInstaller[] DiscoverInstallers(params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
            {
                return Array.Empty<IServiceInstaller>();
            }

            return assemblies
                    .SelectMany(a => a.FindImplementationsOf(typeof(IServiceInstaller)))
                    .SelectNonNull(type =>
                    {
                        try
                        {
                            return Activator.CreateInstance(type) as IServiceInstaller;
                        }
                        catch
                        {
                            return null;
                        }
                    })
                    .ToArray();
        }

        public static IServiceInstaller[] DiscoverInstallers(string assemblyStartsWith)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.StartsWith(assemblyStartsWith, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            return DiscoverInstallers(assemblies);
        }

        private static readonly string[] IgnoreNames = new[] { "bin", "debug", "release", "netcoreapp2.0" };
    }
}
