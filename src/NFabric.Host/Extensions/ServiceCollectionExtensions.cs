namespace NFabric.Core.Extensions
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/> type.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Creates POCO class, binds it with values from configuration and places it in dependecu injection container as singleton.
        /// It a way of how to make configuration if you are not a fan of <see cref="IOptions{TConfig}"/> in your constructors.
        /// </summary>
        public static TConfig AddStronglyTypedConfig<TConfig>(this IServiceCollection services, IConfiguration configuration, string key)
            where TConfig : class, new()
        {
            var config = new TConfig();
            configuration.Bind(key, config);
            services.AddSingleton(config);
            return config;
        }
    }
}
