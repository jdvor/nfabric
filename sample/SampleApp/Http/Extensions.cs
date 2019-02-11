namespace SampleApp.Http
{
    using NFabric.Core.Extensions;
    using NFabric.Core.Http;
    using System.Text;
    using Console = System.Console;

    public static class Extensions
    {
        public static void DumpToConsole<T>(this Response<T> r)
            where T : class
        {
            if (r.IsSuccess)
            {
                Console.WriteLine($"SUCCESS HttpStatus: {r.Status}, Value: {r.Value}");
            }
            else
            {
                var sb = new StringBuilder("ERROR");
                if (r.Status.HasValue)
                {
                    sb.Append($" HttpStatus: {r.Status.Value},");
                }

                if (!string.IsNullOrEmpty(r.Error))
                {
                    sb.Append($" Error: {r.Error}");
                }

                if (r.Exception != null)
                {
                    sb.AppendLine();
                    sb.Append(r.Exception.Describe());
                }

                sb.AppendLine();

                Console.WriteLine(sb);
            }
        }
    }
}
