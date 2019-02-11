namespace NFabric.Core.Extensions
{
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Extension methods for <see cref="IConfiguration"/> type.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Creates and binds POCO class. Used mainly for strongly typed configuration classes.
        /// </summary>
        public static TConfig Bind<TConfig>(this IConfiguration configuration, string key)
            where TConfig : class, new()
        {
            var config = new TConfig();

            if (string.IsNullOrEmpty(key))
            {
                configuration.Bind(config);
            }
            else
            {
                var section = configuration.GetSection(key);
                section?.Bind(config);
            }

            return config;
        }

        /// <summary>
        /// Creates and binds POCO class. Used mainly for strongly typed configuration classes.
        /// </summary>
        public static TConfig Bind<TConfig>(this IConfigurationSection section)
            where TConfig : class, new()
        {
            var config = new TConfig();
            section.Bind(config);
            return config;
        }
    }
}
