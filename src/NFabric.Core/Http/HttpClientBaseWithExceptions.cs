namespace NFabric.Core.Http
{
    using NFabric.Core.Serialization;
    using JetBrains.Annotations;
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class HttpClientBaseWithExceptions : IDisposable
    {
        private readonly HttpClient http;
        private readonly bool disposeHttp;
        private readonly ISerialization serialization;
        private readonly Action<HttpRequestMessage>[] requestModifications;
        private readonly bool hasRequestModifications;
        private const string SerializationErrorMsg = "Serialization (or deserialization) has failed.";
        private const string CancelledByClientErrorMsg = "Request has been cancelled by client.";
        private const string ServerNotThereErrorMsg = "Failed to establish connection to the server.";
        private const string ServerTimeoutErrorMsg = "Server has not responded in time.";
        private const string FallbackErrorMsg = "Uncaught exception {0}";

        protected HttpClientBaseWithExceptions(
            [NotNull] HttpClient http,
            [NotNull] ISerialization serialization,
            bool disposeHttp = true,
            Action<HttpRequestMessage>[] requestModifications = null)
        {
            this.http = http;
            this.disposeHttp = disposeHttp;
            this.serialization = serialization;
            this.requestModifications = requestModifications;
            hasRequestModifications = requestModifications?.Length > 0;

            this.http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(serialization.ContentType));
        }

        protected HttpClientBaseWithExceptions(
            [NotNull] HttpClientBuilder builder,
            [NotNull] ISerialization serialization,
            Action<HttpRequestMessage>[] requestModifications = null)
            : this(builder.Build(), serialization, true, requestModifications)
        {
        }

        protected HttpClientBaseWithExceptions(
            [NotNull] string baseAddress,
            [NotNull] ISerialization serialization,
            Action<HttpRequestMessage>[] requestModifications = null)
            : this(new HttpClientBuilder().BaseAddress(baseAddress), serialization, requestModifications)
        {
        }

        protected async Task<TResponse> SendAsync<TRequest, TResponse>(
            [NotNull] string endpoint,
            [NotNull] TRequest request,
            HttpMethod method = null,
            CancellationToken ct = default(CancellationToken))
            where TRequest : class
            where TResponse : class
        {
            var httpMethod = method ?? HttpMethod.Post;
            Func<HttpRequestMessage> reqFn = () =>
            {
                var bytes = serialization.Serialize(request);
                var content = new ByteArrayContent(bytes);
                content.Headers.ContentType = new MediaTypeHeaderValue(serialization.ContentType);
                var req = new HttpRequestMessage(httpMethod, endpoint) { Content = content };
                return req;
            };
            return await ExecuteAsync<TResponse>(reqFn, ct).ConfigureAwait(false);
        }

        protected async Task<TResponse> SendAsync<TResponse>(
            [NotNull] string endpoint,
            HttpMethod method = null,
            CancellationToken ct = default(CancellationToken))
            where TResponse : class
        {
            var httpMethod = method ?? HttpMethod.Get;
            return await ExecuteAsync<TResponse>(() => new HttpRequestMessage(httpMethod, endpoint), ct).ConfigureAwait(false);
        }

        private async Task<TResponse> ExecuteAsync<TResponse>(Func<HttpRequestMessage> reqFn, CancellationToken ct)
            where TResponse : class
        {
            try
            {
                var req = reqFn();
                if (hasRequestModifications)
                {
                    foreach (var modify in requestModifications)
                    {
                        modify(req);
                    }
                }

                var resp = await http.SendAsync(req, ct).ConfigureAwait(false);

                if (typeof(TResponse) == typeof(Nothing))
                {
                    if (resp.IsSuccessStatusCode)
                    {
                        return Nothing.Instance as TResponse;
                    }

                    throw new HttpException(resp.StatusCode);
                }

                var responseBytes = await resp.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                if (responseBytes.Length > 0)
                {
                    if (TryReadResponse(responseBytes, out TResponse response))
                    {
                        return response;
                    }

                    throw new HttpException(resp.StatusCode, responseBytes);
                }

                throw new HttpException(resp.StatusCode);
            }
            catch (HttpException)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                throw new HttpException(ServerNotThereErrorMsg, ex);
            }
            catch (TaskCanceledException ex) when (ex.CancellationToken == ct)
            {
                throw new HttpException(CancelledByClientErrorMsg, ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new HttpException(ServerTimeoutErrorMsg, ex);
            }
            catch (Exception ex)
            {
                throw new HttpException(string.Format(FallbackErrorMsg, ex.GetType().Name), ex);
            }
        }

        private bool TryReadResponse<TResponse>(byte[] payload, out TResponse response)
            where TResponse : class
        {
            try
            {
                response = serialization.Deserialize<TResponse>(payload);
                return true;
            }
            catch (SerializationException ex)
            {
                response = null;
                return false;
            }
        }

        public static Action<HttpRequestMessage>[] DefaultQueryParam(string key, string value)
        {
            return new Action<HttpRequestMessage>[]
            {
                (HttpRequestMessage req) =>
                {
                    var endpoint = new EndpointBuilder(req.RequestUri).QueryParam(key, value).ToString();
                    var kind = string.CompareOrdinal(endpoint, "://") == 0
                                ? UriKind.Absolute
                                : UriKind.Relative;
                    req.RequestUri = new Uri(endpoint, kind);
                },
            };
        }

        #region IDisposable

        private bool disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (disposeHttp)
                    {
                        http.Dispose();
                    }
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
