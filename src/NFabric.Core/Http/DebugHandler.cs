namespace NFabric.Core.Http
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Output is visible in Output window of Visual Studio.
    /// </summary>
    public sealed class DebugHandler : DelegatingHandler
    {
        private const string Delim = "----------------------------------------------------------------";

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var sb = new StringBuilder(Delim);
            sb.AppendLine();
            try
            {
                await DumpAsync(sb, request).ConfigureAwait(false);
                var sw = Stopwatch.StartNew();
                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                sw.Stop();
                await DumpAsync(sb, response, sw).ConfigureAwait(false);
                return response;
            }
            catch (Exception ex)
            {
                Dump(sb, ex);
                throw;
            }
            finally
            {
                sb.AppendLine(Delim);
                Debug.WriteLine(sb);
            }
        }

        private static async Task DumpAsync(StringBuilder sb, HttpRequestMessage req)
        {
            sb.AppendLine($"REQUEST {req.Method} {req.RequestUri}");

            foreach (KeyValuePair<string, IEnumerable<string>> h in req.Headers)
            {
                var v = string.Join(" ", h.Value);
                sb.AppendLine($"  {h.Key}: {v}");
            }

            if (req.Content != null)
            {
                if (req.Content.Headers != null)
                {
                    foreach (KeyValuePair<string, IEnumerable<string>> h in req.Content.Headers)
                    {
                        var v = string.Join(" ", h.Value);
                        sb.AppendLine($"  {h.Key}: {v}");
                    }
                }

                var body = await req.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!string.IsNullOrEmpty(body))
                {
                    sb.AppendLine(">>>");
                    sb.AppendLine(body);
                    sb.AppendLine("<<<");
                }
            }
        }

        private static async Task DumpAsync(StringBuilder sb, HttpResponseMessage resp, Stopwatch sw)
        {
            sb.AppendLine($"RESPONSE ==> {(int)resp.StatusCode} {resp.StatusCode} ({sw.ElapsedMilliseconds} ms)");

            foreach (KeyValuePair<string, IEnumerable<string>> h in resp.Headers)
            {
                var v = string.Join(" ", h.Value);
                sb.AppendLine($"  {h.Key}: {v}");
            }

            if (resp.Content != null)
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!string.IsNullOrEmpty(body))
                {
                    sb.AppendLine(">>>");
                    sb.AppendLine(body);
                    sb.AppendLine("<<<");
                }
            }
        }

        private static void Dump(StringBuilder sb, Exception ex)
        {
            sb.AppendLine($"==> EXCEPTION {ex.GetType().Name}: {ex.Message}");
        }
    }
}
