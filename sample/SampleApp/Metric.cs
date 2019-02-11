namespace SampleApp
{
    using App.Metrics;
    using App.Metrics.Counter;
    using App.Metrics.Timer;
    using System;

    public static class Metric
    {
        private static CounterOptions SuccessOptions => new CounterOptions
        {
            Name = Names.SuccessCounter,
            MeasurementUnit = Unit.Calls,
            Context = Names.Context,
        };

        internal static void Success(this IMetrics metrics)
        {
            metrics.Measure.Counter.Increment(SuccessOptions);
        }

        private static CounterOptions ErrorOptions => new CounterOptions
        {
            Name = Names.ErrorCounter,
            MeasurementUnit = Unit.Errors,
            Context = Names.Context,
        };

        internal static void Error(this IMetrics metrics)
        {
            metrics.Measure.Counter.Increment(ErrorOptions);
        }

        private static TimerOptions SampleTimerOptions => new TimerOptions
        {
            Name = Names.SampleTimer,
            DurationUnit = TimeUnit.Milliseconds,
            RateUnit = TimeUnit.Milliseconds,
            Context = Names.Context,
        };

        internal static IDisposable SampleTimer(this IMetrics metrics)
        {
            return metrics.Measure.Timer.Time(SampleTimerOptions);
        }

        public static class Names
        {
            public const string Context = "SampleApp";
            public const string SuccessCounter = "success";
            public const string ErrorCounter = "error";
            public const string SampleTimer = "smplTmr";
            public const string AppId = "appId";
        }
    }
}
