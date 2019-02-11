namespace NFabric.Core.Http
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;

    public static class Extensions
    {
        public const string CorrelationIdHeaderName = "X-CorrelationId";
        public const string CorrelationIdPropsName = "CorrelationId";

        public static string GetCorrelationId(this HttpHeaders headers)
        {
            if (headers != null && headers.TryGetValues(CorrelationIdHeaderName, out IEnumerable<string> values))
            {
                var v = values.FirstOrDefault();
                if (!string.IsNullOrEmpty(v))
                {
                    return v;
                }
            }

            return null;
        }

        public static string GetCorrelationId(this IDictionary<string, object> properties)
        {
            if (properties != null && properties.ContainsKey(CorrelationIdPropsName))
            {
                var v = properties[CorrelationIdPropsName] as string;
                if (!string.IsNullOrEmpty(v))
                {
                    return v;
                }
            }

            return null;
        }

        public static Dictionary<string, string> ToDictionary(this HttpHeaders headers)
        {
            return headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.FirstOrDefault());
        }

        public static string GetFirstOrDefaultValue(this HttpHeaders headers, string name)
        {
            return headers.TryGetValues(name, out IEnumerable<string> values)
                ? values.FirstOrDefault()
                : null;
        }

        public static bool IsTextContent(this HttpContent content, bool @default = true)
        {
            var ct = content.Headers.ContentType.MediaType.ToLowerInvariant();
            switch (ct)
            {
                case "application/octet-stream":
                case "application/protobuf":
                case "application/x-protobuf":
                    return false;

                default:
                    return @default;
            }
        }

        public static Encoding GetEncoding(this HttpContent content)
        {
            try
            {
                var chs = content.Headers.ContentType.CharSet;
                if (!string.IsNullOrEmpty(chs))
                {
                    return Encoding.GetEncoding(chs);
                }
            }
            catch
            {
            }

            return null;
        }

        public static EndpointBuilder QueryParam(this string endpoint, string key, object value)
        {
            return new EndpointBuilder(endpoint).QueryParam(key, value);
        }
    }
}
