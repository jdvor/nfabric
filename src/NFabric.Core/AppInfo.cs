namespace NFabric.Core
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;

    public sealed class AppInfo
    {
        public AppInfo(string[] args = null, string name = null, string env = null, string fqdn = null, string ver = null)
        {
            Args = new ReadOnlyCollection<string>(args ?? Array.Empty<string>());

            Name = string.IsNullOrEmpty(name)
                ? GuessAppName()
                : name;
            Expect.SafeName(Name, nameof(name));

            HostFqdn = string.IsNullOrEmpty(fqdn)
                ? Util.GetHostFqdn()
                : fqdn.ToLowerInvariant();

            Host = Util.SimpleHostNameFromFqdn(HostFqdn);

            Env = string.IsNullOrEmpty(env)
                ? GuessEnvFromFqdn(HostFqdn)
                : env.ToLowerInvariant();

            Version = string.IsNullOrEmpty(ver)
                ? Util.GetAppVersion()
                : ver;
        }

        private static string GuessAppName()
        {
            var appName = Util.GuessAppNameFromPath();
            if (string.IsNullOrEmpty(appName))
            {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                throw new ArgumentException("Parameter 'name' is null and no default name was found.", "name");
#pragma warning restore CA2208
            }

            return appName;
        }

        [SuppressMessage(
            "Layout Rules",
            "SA1503:CurlyBracketsMustNotBeOmitted",
            Justification = "It would be much less readable with braces.")]
        private static string GuessEnvFromFqdn(string fqdn)
        {
            var f = fqdn.ToLowerInvariant();

            if (f.Contains(CommonName.Production)) return CommonName.Production;
            if (f.Contains(CommonName.Stage)) return CommonName.Stage;
            if (f.Contains(CommonName.PerformanceTest)) return CommonName.PerformanceTest;
            if (f.Contains(CommonName.Test)) return CommonName.Test;
            return CommonName.Development;
        }

        public string Name { get; }

        public string Env { get; }

        public string HostFqdn { get; }

        public string Host { get; }

        public string Version { get; }

        public ReadOnlyCollection<string> Args { get; }

        public override string ToString()
        {
            return $"{Name} (ver: {Version}, env: {Env}, fqdn: {HostFqdn})";
        }

        public static class CommonName
        {
            public const string Unknown = "unknown";
            public const string Development = "dev";
            public const string Test = "test";
            public const string PerformanceTest = "perf";
            public const string Stage = "stage";
            public const string Production = "prod";
        }
    }
}
