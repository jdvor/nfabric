namespace NFabric.Host.Conventions
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using NFabric.Core.Extensions;
    using Microsoft.Extensions.Configuration;
    using Serilog.Events;

    /// <summary>
    /// Simplified configuration for Serilog sub-system.
    /// </summary>
    public class LogsOptions
    {
        public Uri BaseUri { get; set; } = new Uri("http://telemetry.nanox.cz:9200");

        public string LogsDir { get; set; } = "%LOGS_PATH%";

        public string IndexFormat { get; set; } = "logs_{0:yyyy-MM-dd}";

        public LogEventLevel MinimumLogEventLevel { get; set; } = LogEventLevel.Information;

        public bool FileEnabled { get; set; } = true;

        public bool ElasticEnabled { get; set; } = true;

        public bool ConsoleEnabled { get; set; } = Environment.UserInteractive;

        /// <summary>
        /// An array of exlude expressions.
        /// https://github.com/serilog/serilog-filters-expressions
        /// </summary>
        public string[] Exclude { get; set; }

        public static LogsOptions CreateForLocalDevelopment()
        {
            return new LogsOptions
            {
                FileEnabled = false,
                ElasticEnabled = false,
                MinimumLogEventLevel = LogEventLevel.Debug,
            };
        }

        public static LogsOptions Create(IConfiguration config, string appName)
        {
            LogsOptions options;
            try
            {
                options = config.Bind<LogsOptions>("logs");
            }
            catch (Exception)
            {
                // in case there's no configuration section "logs"
#if DEBUG
                if (Debugger.IsAttached)
                {
                    // ... turn off publishing to elasticsearch and increase default log level
                    options = CreateForLocalDevelopment();
                }
                else
                {
                    // ... target default (production)
                    options = new LogsOptions();
                }
#else
                // ... target default (production)
                options = new LogsOptions();
#endif
            }

            options.LogsDir = NormalizeDir(options.LogsDir, appName);

            return options;
        }

        /// <summary>
        /// Expands enviroment variables in path and appends application name as sub-directory.
        /// If the path ends with something else than enviroment variable; the application name is not appended.
        /// Falls back to system temporary directory (again with application name as sub-directory)
        /// if variable expansion is not successful.
        /// Also removes terminal directory separator if there is any.
        /// </summary>
        private static string NormalizeDir(string path, string appName)
        {
            string p;
            if (path.IndexOf('%') == -1)
            {
                // no env var in path, so return it as is
                p = path;
            }
            else
            {
                var expanded = Environment.ExpandEnvironmentVariables(path);
                if (expanded.IndexOf('%') == -1)
                {
                    // env vars have been translated successfuly
                    if (path.LastIndexOf('%') == path.Length - 1)
                    {
                        // original path ends with env var (it's typically just %LOGS_PATH%)
                        p = Path.Combine(expanded, appName);
                    }
                    else
                    {
                        p = expanded;
                    }
                }
                else
                {
                    // fall back to temp path with application name as sub-directory
                    p = Path.Combine(Path.GetTempPath(), appName);
                }
            }

            return p.RemoveFromEnd(Path.DirectorySeparatorChar);
        }

        public override string ToString()
        {
            return $"dir: {LogsDir}, es: {ElasticEnabled}, uri: {BaseUri}, index: {IndexFormat}, minLvl: {MinimumLogEventLevel}";
        }
    }
}
