namespace NFabric.Core.Http
{
    using System;
    using System.Text;
    using System.Web;

    public sealed class EndpointBuilder
    {
        private readonly StringBuilder sb;
        private bool hasQueryParam;

        public EndpointBuilder(string endpoint = "/")
        {
            sb = new StringBuilder(endpoint);
            hasQueryParam = endpoint.IndexOf('?') > -1;
        }

        public EndpointBuilder(Uri endpoint)
            : this(endpoint.OriginalString)
        {
        }

        public EndpointBuilder QueryParam(string key, object value, bool encode = true)
        {
            if (value == null)
            {
                return this;
            }

            if (!hasQueryParam)
            {
                sb.Append('?');
                hasQueryParam = true;
            }
            else
            {
                sb.Append('&');
            }

            sb.Append(key);
            sb.Append('=');

            if (encode)
            {
                sb.Append(HttpUtility.UrlEncode(value.ToString()));
            }
            else
            {
                sb.Append(value.ToString());
            }

            return this;
        }

        public EndpointBuilder QueryParams(params (string, object)[] queryParams)
        {
            if (queryParams == null)
            {
                return this;
            }

            foreach (var (key, value) in queryParams)
            {
                if (value == null)
                {
                    continue;
                }

                QueryParam(key, value);
            }

            return this;
        }

        public override string ToString()
        {
            return sb.ToString();
        }

        public static implicit operator string(EndpointBuilder eb)
        {
            return eb?.ToString() ?? string.Empty;
        }
    }
}
