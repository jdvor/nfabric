# .NET Core Fabric - HTTP

## HttpClientBuilder

Builder object with fluent API for constructing HttpClient. It also have several quite sensible defaults like auto-redirection on, short client timeout (10s), etc.

``` csharp
using NFabric.Core.Http;

var http = new HttpClientBuilder()
            .BaseAddress(@"http://www.omdbapi.com")
            .BasicAuthentication("user", "password")
            .MaxRedirects(1)
            .Timeout(TimeSpan.FromSeconds(5))
            .With(new DebugHandler())
            .Build();
```

Methods for:
* Injecting `HttpClient` middleware
* Managing redirects
* Turning off cache
* Setting up basic authentication
* Configuring automatic HTTP headers

## HttpClient middleware

Injected using `HttpClientBuilder.With` method.
These are already available in `Fabric.Core.Http`:

* `CaptureToFilehandler` - captures body of request & response as files into chosen directory in which it creates subdirectories based on date.
* `DebugHandler` - outputs details about request & response to _Output window_ of Visual Studio.
* `MetricAndLoggingHandler` - sends telemetry data upstream using _App.Metrics_ library.
* `RetryHandler` - on certain exceptions it re-tries the request according to chosen strategy (`Func<IRetryIntervals>`).


## HttpClientBase

It is an abstract base class for building clients based on HTTP protocol.

``` csharp
using NFabric.Core.Http;
using NFabric.Core.Serialization;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;

public sealed class OmdbClient : HttpClientBase
{
    public OmdbClient()
        : base(CreateBuilder(), new JsonSerialization(), DefaultQueryParam("apikey", "ed58dfb"))
    {
    }

    public async Task<Response<OmdbSearchResult>> SearchMovieAsync(string name, int year)
    {
        var endpoint = "/".QueryParam("t", name).QueryParam("y", year);
        return await SendAsync<OmdbSearchResult>(endpoint).ConfigureAwait(false);
    }

    private static HttpClientBuilder CreateBuilder()
    {
        return new HttpClientBuilder()
            .BaseAddress(@"http://www.omdbapi.com")
            .MaxRedirects(1)
            .Timeout(TimeSpan.FromSeconds(5))
            .NoCache()
            .With(new DebugHandler());
    }
}
```

Out-of-the-box when extending HttpClientBase:
* Does not throw exception, but always returns `Response` object with (properties like `.IsSuccess`, `.Value`, `.Error`, etc.)
* Allows to set default HTTP headers or query parameters. Default in this case means automatically set on every request.
* Sets `Accept` and `Content-Type` headers based on chosen serialization `ISerialization`.
* Can use `HttpClientBuilder` and therefore have all its features (like handler middleware).
* Takes care about serializing strongly typed request / response, so client does not have to implement it.