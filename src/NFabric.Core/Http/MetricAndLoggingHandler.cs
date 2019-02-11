namespace NFabric.Core.Http
{
    using App.Metrics;
    using App.Metrics.Counter;
    using App.Metrics.Timer;
    using NFabric.Core.Extensions;
    using Serilog;
    using Serilog.Context;
    using Serilog.Events;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class MetricAndLoggingHandler : DelegatingHandler
    {
        private readonly string context;
        private readonly IMetrics metrics;
        private readonly ILogger logger;
        private readonly ICallAnalyzer analyzer;

        public bool LogBody { get; set; }

        public int MaxLoggedBodyLength { get; set; } = 65536; // 64kB

        public MetricAndLoggingHandler(
            IMetrics metrics,
            ILogger logger,
            ICallAnalyzer analyzer)
        {
            this.metrics = metrics;
            this.analyzer = analyzer;
            this.logger = logger;
            context = analyzer.MetricContextName;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var correlationId = GetOrCreateCorrelationId(request, out bool existsInHeaders);
            if (!existsInHeaders)
            {
                request.Headers.Add(Extensions.CorrelationIdHeaderName, correlationId);
            }

            var callName = analyzer.GetCallName(request);
            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("CallName", callName))
            using (OperationTimer(callName))
            {
                try
                {
                    var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                    MarkHttpStatus(callName, response.StatusCode);
                    if (analyzer.IsResponseSuccessful(response))
                    {
                        MarkSuccess(callName);
                        await LogAsync(LogEventLevel.Debug, callName, request, response).ConfigureAwait(false);
                    }
                    else
                    {
                        MarkError(callName);
                        await LogAsync(LogEventLevel.Warning, callName, request, response).ConfigureAwait(false);
                    }

                    return response;
                }
                catch (Exception ex)
                {
                    MarkError(callName);
                    await LogErrorAsync(callName, request, ex).ConfigureAwait(false);
                    throw;
                }
            }
        }

        private static string GetOrCreateCorrelationId(HttpRequestMessage request, out bool existsInHeaders)
        {
            var correlationId = request.Headers.GetCorrelationId();
            if (correlationId != null)
            {
                existsInHeaders = true;
                return correlationId;
            }

            correlationId = request.Properties.GetCorrelationId();
            if (correlationId != null)
            {
                existsInHeaders = false;
                return correlationId;
            }

            existsInHeaders = false;
            return Guid.NewGuid().ToString();
        }

        private IDisposable OperationTimer(string op)
        {
            var opts = new TimerOptions
            {
                Name = "Elapsed",
                DurationUnit = TimeUnit.Milliseconds,
                RateUnit = TimeUnit.Milliseconds,
                Context = context,
            };
            return metrics.Measure.Timer.Time(opts);
        }

        private void MarkHttpStatus(string op, HttpStatusCode status)
        {
            var opts = new CounterOptions
            {
                Name = "HttpStatus",
                MeasurementUnit = Unit.Calls,
                Context = context,
            };
            metrics.Measure.Counter.Increment(opts);
        }

        private void MarkSuccess(string op)
        {
            var opts = new CounterOptions
            {
                Name = "Success",
                MeasurementUnit = Unit.Calls,
                Context = context,
            };
            metrics.Measure.Counter.Increment(opts);
        }

        private void MarkError(string op)
        {
            var opts = new CounterOptions
            {
                Name = "Error",
                MeasurementUnit = Unit.Errors,
                Context = context,
            };
            metrics.Measure.Counter.Increment(opts);
        }

        private async Task LogAsync(LogEventLevel level, string op, HttpRequestMessage rq, HttpResponseMessage rs)
        {
            var code = (int)rs.StatusCode;
            var verb = (int)level > (int)LogEventLevel.Information ? "failed" : "succeeded";
            var pattern = new StringBuilder($"HTTP call {{Call}} {{Method}} {{RequestUri}} {verb} with {{HttpStatusCode}} {rs.StatusCode}.");
            var @params = new List<object> { op, rq.Method, rq.RequestUri.AbsoluteUri, code };

            var rqHeaders = rq.Headers.ToDictionary();
            if (rqHeaders.Count > 0)
            {
                pattern.AppendLine();
                pattern.Append("RequestHeaders: {RequestHeaders}");
                @params.Add(rqHeaders);
            }

            if (LogBody)
            {
                var rqBody = await GetBodyAsStringAndRewindAsync(rq.Content).ConfigureAwait(false);
                if (rqBody != null)
                {
                    pattern.AppendLine();
                    pattern.Append("RequestBody >>>");
                    pattern.AppendLine();
                    pattern.Append("{RequestBody}");
                    @params.Add(rqBody);
                }
            }

            var rsHeaders = rs.Headers.ToDictionary();
            if (rsHeaders.Count > 0)
            {
                pattern.AppendLine();
                pattern.Append("ResponseHeaders: {ResponseHeaders}");
                @params.Add(rsHeaders);
            }

            if (LogBody)
            {
                var rsBody = await GetBodyAsStringAndRewindAsync(rs.Content).ConfigureAwait(false);
                if (rsBody != null)
                {
                    pattern.AppendLine();
                    pattern.Append("ResponseBody >>>");
                    pattern.AppendLine();
                    pattern.Append("{ResponseBody}");
                    @params.Add(rsBody);
                }
            }

            logger.Write(level, pattern.ToString(), @params.ToArray());
        }

        private async Task LogErrorAsync(string op, HttpRequestMessage rq, Exception ex)
        {
            var pattern = new StringBuilder($"HTTP call {{Call}} {{Method}} {{RequestUri}} failed with {ex.GetType().Name}.");
            var @params = new List<object> { op, rq.Method, rq.RequestUri.AbsoluteUri };

            var rqHeaders = rq.Headers.ToDictionary();
            if (rqHeaders.Count > 0)
            {
                pattern.AppendLine();
                pattern.Append("RequestHeaders: {RequestHeaders}");
                @params.Add(rqHeaders);
            }

            if (LogBody)
            {
                var rqBody = await GetBodyAsStringAndRewindAsync(rq.Content).ConfigureAwait(false);
                if (rqBody != null)
                {
                    pattern.AppendLine();
                    pattern.Append("RequestBody >>>");
                    pattern.AppendLine();
                    pattern.Append("{RequestBody}");
                    @params.Add(rqBody);
                }
            }

            logger.Error(ex, pattern.ToString(), @params.ToArray());
        }

        private async Task<string> GetBodyAsStringAndRewindAsync(HttpContent content)
        {
            try
            {
                var stream = await content.ReadAsStreamAsync().ConfigureAwait(false);
                if (stream != null)
                {
                    if (content.IsTextContent())
                    {
                        var encoding = content.GetEncoding();
                        var str = await stream.ReadAllTextAndRewindAsync(encoding).ConfigureAwait(false);
                        return str.Length <= MaxLoggedBodyLength
                            ? str
                            : str.Substring(0, MaxLoggedBodyLength);
                    }
                    else
                    {
                        var bytes = await stream.ReadAllAndRewindAsync().ConfigureAwait(false);
                        return BitConverter.ToString(bytes, 0, MaxLoggedBodyLength).Replace("-", string.Empty);
                    }
                }
            }
            catch
            {
            }

            return null;
        }
    }
}
