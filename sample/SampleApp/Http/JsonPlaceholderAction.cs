namespace SampleApp.Http
{
    using App.Metrics;
    using NFabric.Core;
    using NFabric.Core.Attributes;
    using Microsoft.Extensions.Configuration;
    using Serilog;
    using System.Threading;
    using System.Threading.Tasks;

    [ServiceName(ActionName)]
    public class JsonPlaceholderAction : IAction
    {
        public const string ActionName = "jp";

        public string Name => ActionName;

        private readonly IMetrics metrics;
        private readonly ILogger logger;

        public JsonPlaceholderAction(IMetrics metrics, ILogger logger)
        {
            this.metrics = metrics;
            this.logger = logger;
        }

        public async Task ExecuteAsync(IConfiguration config, CancellationToken cancellationToken = default(CancellationToken))
        {
            ////DebugEventHandler.Instance.CallMade += OnCallMade;

            using (var jpc = new JsonPlaceholderClient(metrics, logger))
            {
                // var response = jpc.GetAllPostsAsync().ConfigureAwait(false);
                var response = await jpc.CreatePostsAsync(1, "Lorem ipsum", "Lorem ipsum dolor sit").ConfigureAwait(false);
                response.DumpToConsole();
            }
        }

        ////private static void OnCallMade(object sender, CallMadeEventArgs e)
        ////{
        ////    var desc = e.Description;
        ////}
    }
}
