namespace NFabric.Core.Extensions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Serilog;

    /// <summary>
    /// Extension methods for <see cref="ILogger"/> type.
    /// </summary>
    public static class SerilogExtensions
    {
        /// <summary>
        /// Finds all exceptions if failed tasks and logs them as errors.
        /// </summary>
        public static void ExceptionsAsError(this ILogger logger, IEnumerable<Task> tasks, string message)
        {
            foreach (var ex in tasks.ExtractExceptions())
            {
                logger.Error(ex, message);
            }
        }

        /// <summary>
        /// Finds all exceptions if failed tasks and logs them as warnings.
        /// </summary>
        public static void ExceptionsAsWarning(this ILogger logger, IEnumerable<Task> tasks, string message)
        {
            foreach (var ex in tasks.ExtractExceptions())
            {
                logger.Warning(ex, message);
            }
        }

        /// <summary>
        /// Finds all exceptions if failed tasks and logs them as information events.
        /// </summary>
        public static void ExceptionsAsInfo(this ILogger logger, IEnumerable<Task> tasks, string message)
        {
            foreach (var ex in tasks.ExtractExceptions())
            {
                logger.Information(ex, message);
            }
        }

        /// <summary>
        /// Finds all exceptions if failed tasks and logs them as debug events.
        /// </summary>
        public static void ExceptionsAsDebug(this ILogger logger, IEnumerable<Task> tasks, string message)
        {
            foreach (var ex in tasks.ExtractExceptions())
            {
                logger.Debug(ex, message);
            }
        }
    }
}
