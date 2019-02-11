using System.Net.Http;

namespace NFabric.Core.Http
{
    public interface ICallAnalyzer
    {
        string GetCallName(HttpRequestMessage request);

        bool IsResponseSuccessful(HttpResponseMessage response);

        string MetricContextName { get; }
    }
}
