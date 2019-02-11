namespace SampleApp.Http
{
    using NFabric.Core.Http;
    using NFabric.Core.Serialization;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// http://www.omdbapi.com/
    /// </summary>
    public sealed class OmdbClient : HttpClientBase
    {
        public OmdbClient()
            : base(CreateBuilder(), new JsonSerialization(), DefaultQueryParam("apikey", "ed58dfb"))
        {
        }

        public async Task<Response<OmdbSearchResult>> SearchMovieAsync(string name, int year)
        {
            var ep = "/".QueryParam("t", name).QueryParam("y", year);
            return await SendAsync<OmdbSearchResult>(ep).ConfigureAwait(false);
        }

        private static HttpClientBuilder CreateBuilder()
        {
            return new HttpClientBuilder()
                .BaseAddress(@"http://www.omdbapi.com")
                .MaxRedirects(1)
                .NoCache()
                .Timeout(TimeSpan.FromSeconds(5))
                .With(new DebugHandler());
        }
    }
}
