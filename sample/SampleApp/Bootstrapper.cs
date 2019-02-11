namespace SampleApp
{
    using NFabric.Core;
    using NFabric.Core.Extensions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    internal class Bootstrapper : IServiceInstaller
    {
        public void Install(IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<IHostedService, SamplePeriodicService>();

            var assembly = typeof(Program).Assembly;
            services.AddTransients<IAction>(assembly);

            var muClass = config.Bind<MuClass>("mu");
            services.AddSingleton(muClass);
        }
    }
}
