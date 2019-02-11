namespace NFabric.Core.Messaging
{
    using NFabric.Core.Extensions;
    using System;
    using System.Collections.Generic;

    public sealed class MessageContext
    {
        public MessageContext(
            string correlationId,
            string contentType,
            string enclosedType,
            int retryCount,
            IDictionary<string, object> props)
        {
            CorrelationId = correlationId;
            ContentType = contentType;
            EnclosedType = enclosedType;
            RetryCount = retryCount;
            Props = props ?? new Dictionary<string, object>(0);
        }

        public static MessageContext Create<T>(
            string correlationId = null,
            string contentType = null,
            int retryCount = 0,
            IDictionary<string, object> props = null)
        {
            return new MessageContext(
                correlationId ?? Guid.NewGuid().ToString(),
                contentType ?? ContentTypes.Json,
                typeof(T).GetRelaxedFullName(),
                retryCount,
                props);
        }

        public string CorrelationId { get; }

        public string ContentType { get; }

        public string EnclosedType { get; }

        public int RetryCount { get; }

        public IDictionary<string, object> Props { get; }

        public bool HasEnclosedType => EnclosedType != null;

        public override string ToString()
        {
            return $"message {CorrelationId} declared with '{EnclosedType}'";
        }
    }
}
