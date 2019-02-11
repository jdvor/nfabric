namespace NFabric.Host.Conventions
{
    using System;
    using System.IO;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using Serilog.Sinks.Elasticsearch;
    using Serilog.Sinks.SystemConsole.Themes;
    using ILogger = Serilog.ILogger;
    using NFabric.Core;
    using Serilog.Events;

    public static class DefaultLogging
    {
        internal const string FileTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} | {Level:u3} | " +
            "{Message:l} | {SourceContext} | thread {ThreadId}{NewLine}{Exception}";

        public static (ILoggerFactory, ILogger) Create(LogsOptions opts, AppInfo appInfo)
        {
            var builder = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithDemystifiedStackTraces();

            if (opts.FileEnabled || opts.ElasticEnabled)
            {
                builder
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("App", appInfo.Name)
                    .Enrich.WithProperty("Env", appInfo.Env)
                    .Enrich.WithProperty("Host", appInfo.Host)
                    .Enrich.WithProperty("AppVer", appInfo.Version)
                    .Enrich.WithThreadId();
            }

            if (opts.Exclude?.Length > 0)
            {
                foreach (var expression in opts.Exclude)
                {
                    builder.Filter.ByExcluding(expression);
                }
            }

            if (opts.FileEnabled)
            {
                if (!Directory.Exists(opts.LogsDir))
                {
                    Directory.CreateDirectory(opts.LogsDir);
                }

                builder.WriteTo.Async(
                    x => x.RollingFile(
                        pathFormat: $"{opts.LogsDir}\\{{Date}}.log",
                        fileSizeLimitBytes: 104857600, // 100MB
                        retainedFileCountLimit: 310,
                        restrictedToMinimumLevel: LogEventLevel.Debug,
                        outputTemplate: FileTemplate),
                    bufferSize: 3000,
                    blockWhenFull: false);
            }

            if (opts.ConsoleEnabled)
            {
                builder.WriteTo.Console(
                    restrictedToMinimumLevel: opts.MinimumLogEventLevel,
                    theme: AnsiConsoleTheme.Code);
            }

            if (opts.ElasticEnabled)
            {
                var esOpts = new ElasticsearchSinkOptions(opts.BaseUri)
                {
                    MinimumLogEventLevel = opts.MinimumLogEventLevel,
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
                    IndexFormat = opts.IndexFormat,
                };
                builder.WriteTo.Elasticsearch(esOpts);
            }

            Log.Logger = builder.CreateLogger();
            var loggerFactory = new LoggerFactory().AddSerilog(dispose: true);

            return (loggerFactory, Log.Logger);
        }

        public static void ConfigureServices(IServiceCollection services, ILoggerFactory loggerFactory, ILogger defaultLogger)
        {
            services.AddSingleton(loggerFactory);
            services.AddSingleton(defaultLogger);
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddLogging();
        }
    }
}
