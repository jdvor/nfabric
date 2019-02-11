namespace SampleApp.Http
{
    using App.Metrics;
    using NFabric.Core.Http;
    using NFabric.Core.Serialization;
    using Serilog;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// http://www.omdbapi.com/
    /// </summary>
    public sealed class JsonPlaceholderClient : HttpClientBase
    {
        public JsonPlaceholderClient(IMetrics metrics, ILogger logger)
            : base(CreateBuilder(metrics, logger), new JsonSerialization())
        {
        }

        public async Task<Response<JsonPlaceholderPost[]>> GetAllPostsAsync()
        {
            return await SendAsync<JsonPlaceholderPost[]>("/posts").ConfigureAwait(false);
        }

        public async Task<Response<JsonPlaceholderPost>> CreatePostsAsync(int userId, string title, string body)
        {
            var newPost = new JsonPlaceholderPost
            {
                UserId = userId,
                Title = title,
                Body = body,
            };
            return await SendAsync<JsonPlaceholderPost, JsonPlaceholderPost>("/posts", newPost).ConfigureAwait(false);
        }

        private static HttpClientBuilder CreateBuilder(IMetrics metrics, ILogger logger)
        {
            var analyzer = new JsonPlaceholderCallAnalyzer();

            return new HttpClientBuilder()
                .BaseAddress(@"https://jsonplaceholder.typicode.com")
                .MaxRedirects(1)
                .Timeout(TimeSpan.FromSeconds(5))
                ////.With(DebugEventHandler.Instance);
                .With(new MetricAndLoggingHandler(metrics, logger.ForContext<JsonPlaceholderClient>(), analyzer));
                ////.With(new DebugHandler());
                ////.With(new CaptureToFileHandler(analyzer, @"%LOGS_PATH%\JsonPlaceholderClient"));
        }
    }
}
