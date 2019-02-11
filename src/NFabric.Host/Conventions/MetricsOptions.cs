namespace NFabric.Host.Conventions
{
    using System;
    using System.Diagnostics;
    using NFabric.Core.Extensions;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Simplified configuration for App.Metrics sub-system.
    /// </summary>
    public class MetricsOptions
    {
        public Uri BaseUri { get; set; } = new Uri("http://telemetry.nanox.cz:8086");

        public string Database { get; set; } = "metrics";

        public bool CreateDataBaseIfNotExists { get; set; } = true;

        public TimeSpan FlushInterval { get; set; } = TimeSpan.FromSeconds(10);

        public int FailuresBeforeBackoff { get; set; } = 3;

        public TimeSpan BackoffInterval { get; set; } = TimeSpan.FromSeconds(60);

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(6);

        public bool Enabled { get; set; } = true;

        public bool ReportingEnabled { get; set; } = true;

        public static MetricsOptions CreateWithReportingTurnedOff()
        {
            return new MetricsOptions
            {
                Enabled = false,
                ReportingEnabled = false,
                BaseUri = null,
                Database = null,
                FlushInterval = TimeSpan.Zero,
                FailuresBeforeBackoff = 0,
                BackoffInterval = TimeSpan.Zero,
                Timeout = TimeSpan.Zero,
            };
        }

        public static MetricsOptions Create(IConfiguration config)
        {
            try
            {
                return config.Bind<MetricsOptions>("metrics");
            }
            catch (Exception)
            {
                // in case there's no configuration section "metrics"
#if DEBUG
                if (Debugger.IsAttached)
                {
                    // ... turn off reporting
                    return CreateWithReportingTurnedOff();
                }
#endif

                // ... target default (production)
                return new MetricsOptions();
            }
        }

        public override string ToString()
        {
            return $"enabled: {Enabled}, uri: {BaseUri}, db: {Database}";
        }
    }
}
