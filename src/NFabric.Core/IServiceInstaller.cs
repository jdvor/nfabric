namespace NFabric.Core
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Defines classes which could be used as installers of services to
    /// <see cref="Microsoft.Extensions.DependencyInjection"/> container.
    /// </summary>
    public interface IServiceInstaller
    {
        void Install(IServiceCollection services, IConfiguration config);
    }
}
