namespace NFabric.Host.Conventions
{
    using NFabric.Core.Extensions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class DefaultConfiguration
    {
        public static IConfiguration Create(IEnumerable<string> configFilePaths = null, string[] commandLineArgs = null)
        {
            var cfg = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory);

            var configs = Configs(configFilePaths);
            foreach (var path in configs)
            {
                cfg.AddJsonFile(path, optional: true, reloadOnChange: false);
            }

            cfg.AddEnvironmentVariables();

            if (commandLineArgs != null)
            {
                cfg.AddCommandLine(commandLineArgs);
            }

            return cfg.Build();
        }

        public static void Configure(IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton(config);
            services.AddOptions();
        }

        private static string[] Configs(IEnumerable<string> configFilePaths)
        {
            if (configFilePaths == null)
            {
                return Array.Empty<string>();
            }

            return configFilePaths.Where(x => !string.IsNullOrEmpty(x)).ToArray();
        }
    }
}
