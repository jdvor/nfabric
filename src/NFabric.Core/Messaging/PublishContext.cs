namespace NFabric.Core.Messaging
{
    using System.Collections.Generic;

    public sealed class PublishContext
    {
        public string CorrelationId { get; set; }

        public string Exchnage { get; set; }

        public string RoutingKey { get; set; }

        public int RetryCount { get; set; }

        public IDictionary<string, string> Props { get; set; }
    }
}
