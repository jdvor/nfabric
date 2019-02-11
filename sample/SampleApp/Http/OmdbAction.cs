namespace SampleApp.Http
{
    using NFabric.Core;
    using NFabric.Core.Attributes;
    using Microsoft.Extensions.Configuration;
    using System.Threading;
    using System.Threading.Tasks;

    [ServiceName(ActionName)]
    public class OmdbAction : IAction
    {
        public const string ActionName = "omdb";

        public string Name => ActionName;

        public async Task ExecuteAsync(IConfiguration config, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var omdb = new OmdbClient())
            {
                var response = await omdb.SearchMovieAsync("Hell", 2015).ConfigureAwait(false);
                response.DumpToConsole();
            }
        }
    }
}
