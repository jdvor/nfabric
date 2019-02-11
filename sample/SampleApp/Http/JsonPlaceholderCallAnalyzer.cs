namespace SampleApp.Http
{
    using NFabric.Core.Http;
    using System.Net;
    using System.Net.Http;

    public class JsonPlaceholderCallAnalyzer : ICallAnalyzer
    {
        public string MetricContextName => "JsonPlaceholder";

        public string GetCallName(HttpRequestMessage request)
        {
            var url = request.RequestUri.OriginalString;
            if (url.IndexOf("/posts") > -1)
            {
                switch (request.Method.ToString())
                {
                    case "GET":
                        return "GetPosts";

                    case "POST":
                        return "CreatePost";
                }
            }

            return "Other";
        }

        public bool IsResponseSuccessful(HttpResponseMessage response)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                case HttpStatusCode.Created:
                    return true;

                default:
                    return false;
            }
        }
    }
}
