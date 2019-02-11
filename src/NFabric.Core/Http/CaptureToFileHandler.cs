namespace NFabric.Core.Http
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class CaptureToFileHandler : DelegatingHandler
    {
        private readonly ICallAnalyzer analyzer;
        private readonly DirectoryInfo directory;
        private readonly bool silentOnErrors;

        public CaptureToFileHandler(ICallAnalyzer analyzer, string directory, bool silentOnErrors = true)
        {
            this.analyzer = analyzer;
            this.directory = new DirectoryInfo(Environment.ExpandEnvironmentVariables(directory));
            this.silentOnErrors = silentOnErrors;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.Now;
            var dir = Path.Combine(directory.FullName, now.ToString("yyyy-MM-dd"));
            Directory.CreateDirectory(dir); // checks if it exists internally
            var baseFileName = CreateBaseFileName(request, now);
            await CaptureAsync(request, dir, baseFileName).ConfigureAwait(false);

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            await CaptureAsync(response, dir, baseFileName).ConfigureAwait(false);

            return response;
        }

        private async Task CaptureAsync(HttpRequestMessage request, string dir, string baseFileName)
        {
            try
            {
                var bytes = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                if (bytes?.Length > 0)
                {
                    var suffix = CreateSuffix(request.Content);
                    var path = Path.Combine(dir, $"{baseFileName}_req.{suffix}");
                    File.WriteAllBytes(path, bytes);
                }
            }
            catch
            {
                if (!silentOnErrors)
                {
                    throw;
                }
            }
        }

        private async Task CaptureAsync(HttpResponseMessage response, string dir, string baseFileName)
        {
            try
            {
                var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                if (bytes?.Length > 0)
                {
                    var suffix = CreateSuffix(response.Content);
                    var path = Path.Combine(dir, $"{baseFileName}_resp.{suffix}");
                    File.WriteAllBytes(path, bytes);
                }
            }
            catch
            {
                if (!silentOnErrors)
                {
                    throw;
                }
            }
        }

        private string CreateBaseFileName(HttpRequestMessage request, DateTimeOffset now)
        {
            var callName = analyzer.GetCallName(request);
            var correlationId =
                request.Headers.GetCorrelationId() ??
                request.Properties.GetCorrelationId() ??
                now.ToString("yyyyMMddHHmmssfff");

            return $"{callName}_{correlationId}";
        }

        private static string CreateSuffix(HttpContent content)
        {
            var ct = content.Headers.ContentType.MediaType.ToLowerInvariant();
            switch (ct)
            {
                case "text/json":
                case "application/json":
                    return "json";

                case "text/xml":
                case "application/xml":
                    return "xml";

                case "application/protobuf":
                case "application/x-protobuf":
                    return "pb";

                case "application/octet-stream":
                    return "bin";

                default:
                    return "txt";
            }
        }
    }
}
