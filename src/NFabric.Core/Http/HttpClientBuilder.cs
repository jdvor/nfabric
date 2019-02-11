namespace NFabric.Core.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;

    public class HttpClientBuilder
    {
        private HttpClientHandler httpHandler;
        private Uri baseAddress;
        private TimeSpan? connectionLeaseTimeout;
        private TimeSpan timeout = TimeSpan.FromSeconds(10);
        private Action<HttpRequestHeaders> configureHeaders;
        private AuthenticationHeaderValue authentication;
        private CredentialCache credentials;
        private int? redirects;
        private CacheControlHeaderValue noCache;
        private readonly List<DelegatingHandler> decorators = new List<DelegatingHandler>();

        public Uri BaseUri => baseAddress;

        public HttpClient Build()
        {
            DelegatingHandler prev = null;
            foreach (var current in decorators)
            {
                if (prev != null)
                {
                    prev.InnerHandler = current;
                }

                prev = current;
            }

            var httpHandler = this.httpHandler ?? DefaultClientHandler();
            var hasDecorators = decorators.Count > 0;
            if (hasDecorators)
            {
                decorators.Last().InnerHandler = httpHandler;
            }

            var handler = hasDecorators
                            ? (HttpMessageHandler)decorators.First()
                            : httpHandler;
            var client = new HttpClient(handler)
            {
                Timeout = timeout,
            };

            if (baseAddress != null)
            {
                client.BaseAddress = baseAddress;
                if (connectionLeaseTimeout.HasValue)
                {
                    var sp = ServicePointManager.FindServicePoint(baseAddress);
                    sp.ConnectionLeaseTimeout = connectionLeaseTimeout.Value.Milliseconds;
                }
            }

            configureHeaders?.Invoke(client.DefaultRequestHeaders);

            if (authentication != null)
            {
                client.DefaultRequestHeaders.Authorization = authentication;
            }

            if (noCache != null)
            {
                client.DefaultRequestHeaders.CacheControl = noCache;
            }

            this.httpHandler = null;
            decorators.Clear();

            return client;
        }

        public HttpClientBuilder Use(HttpClientHandler httpHandler)
        {
            this.httpHandler = httpHandler;
            return this;
        }

        private HttpClientHandler DefaultClientHandler()
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = redirects.HasValue && redirects.Value != 0,
                MaxAutomaticRedirections = redirects ?? 3,
            };

            if (credentials != null)
            {
                handler.Credentials = credentials;
            }

            return handler;
        }

        public HttpClientBuilder With(DelegatingHandler handlerDecorator)
        {
            if (!decorators.Contains(handlerDecorator))
            {
                decorators.Add(handlerDecorator);
            }

            return this;
        }

        public HttpClientBuilder BaseAddress(string baseAddress, TimeSpan? connectionLeaseTimeout = null)
        {
            this.baseAddress = new Uri(baseAddress);
            this.connectionLeaseTimeout = connectionLeaseTimeout;
            return this;
        }

        public HttpClientBuilder Timeout(TimeSpan timeout)
        {
            this.timeout = timeout;
            return this;
        }

        public HttpClientBuilder MaxRedirects(int redirects)
        {
            this.redirects = redirects;
            return this;
        }

        public HttpClientBuilder DisableRedirects()
        {
            redirects = 0;
            return this;
        }

        public HttpClientBuilder BasicAuthentication(string username, string password)
        {
            authentication = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}")));
            return this;
        }

        public HttpClientBuilder DigestAuthentication(string username, string password)
        {
            credentials = new CredentialCache
            {
                { baseAddress, "Digest", new NetworkCredential(username, password) },
            };
            return this;
        }

        public HttpClientBuilder OpenAuthentication(string bearerToken)
        {
            authentication = new AuthenticationHeaderValue("Bearer", bearerToken);
            return this;
        }

        public HttpClientBuilder NoCache()
        {
            noCache = new CacheControlHeaderValue { Private = true, NoCache = true };
            return this;
        }

        public HttpClientBuilder ConfigureDefaultHeaders(Action<HttpRequestHeaders> configureHeaders)
        {
            this.configureHeaders = configureHeaders;
            return this;
        }
    }
}
