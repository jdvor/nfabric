namespace NFabric.Core.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Extension methods for<see cref="Task"/> and <see cref="IEnumerable{Task}"/> types.
    /// </summary>
    public static class TasksExtensions
    {
        /// <summary>
        /// Safely execute the Task without waiting for it to complete before moving to the next line of code; commonly known as "Fire And Forget".
        /// Inspired by John Thiriet's blog post, "Removing Async Void": https://johnthiriet.com/removing-async-void/.
        /// </summary>
        /// <param name="task">Task.</param>
        /// <param name="continueOnCapturedContext">
        ///     If set to <c>true</c> continue on captured context; this will ensure that the Synchronization Context returns to the calling thread.
        ///     If set to <c>false</c> continue on a different context; this will allow the Synchronization Context to continue on a different thread.
        /// </param>
        /// <param name="onException">
        ///     If an exception is thrown in the Task, <c>onException</c> will execute.
        ///     If onException is null, the exception will be re-thrown.
        /// </param>
        #pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        #pragma warning disable CA1030 // Use events where appropriate
        public static async void FireAndForget(this Task task, bool continueOnCapturedContext = false, Action<Exception> onException = null)
        #pragma warning restore CA1030
        #pragma warning restore RECS0165
        {
            try
            {
                await task.ConfigureAwait(continueOnCapturedContext);
            }
            catch (Exception ex)
            {
                if (onException == null)
                {
                    throw;
                }

                onException.Invoke(ex);
            }
        }

        /// <summary>
        /// Stops waiting for a task when <paramref name="ct"/> is cancelled.
        /// This is meant for asynchronous tasks which do not support cancellation.
        /// </summary>
        /// <remarks>
        /// Be aware that some classes do have cancellation semantic,
        /// but in reality do not support it (for example: System.Net.Sockets.NetworkStream.ReadAsync).
        /// </remarks>
        public static async Task<T> WithWaitCancellation<T>(this Task<T> task, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (ct.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
            {
                if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false))
                {
                    throw new OperationCanceledException(ct);
                }
            }

            return await task.ConfigureAwait(false);
        }

        /// <summary>
        /// Stops waiting for after a period of time (<paramref name="timeout"/>).
        /// This is meant for asynchronous tasks which do not support cancellation.
        /// </summary>
        /// <remarks>
        /// /// <remarks>
        /// Be aware that some classes do have cancellation semantic,
        /// but in reality do not support it (for example: System.Net.Sockets.NetworkStream.ReadAsync).
        /// </remarks>
        public static async Task<T> WithWaitCancellation<T>(this Task<T> task, TimeSpan timeout)
        {
            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(timeout);
                return await WithWaitCancellation(task, cts.Token).ConfigureAwait(false);
            }
        }

        public static bool IsSuccessful(this Task task)
        {
            return task.IsCompleted && !task.IsCanceled && !task.IsFaulted;
        }

        /// <summary>
        /// Returns if all tasks have been successfuly completed.
        /// </summary>
        public static bool HasAllSuccessfulyCompleted(this IEnumerable<Task> tasks)
        {
            return (tasks == null)
                ? false
                : tasks.All(x => x.IsCompleted && !x.IsFaulted && !x.IsCanceled);
        }

        /// <summary>
        /// Extracts exceptions from failed tasks.
        /// </summary>
        public static IEnumerable<Exception> ExtractExceptions(this IEnumerable<Task> tasks)
        {
            return tasks == null
                ? Array.Empty<Exception>()
                : tasks.Where(x => x.Exception != null).SelectMany(x => x.Exception.Flatten().InnerExceptions);
        }

        /// <summary>
        /// Returns all successfuly completed tasks.
        /// </summary>
        public static IEnumerable<Task> Successful(this IEnumerable<Task> tasks)
        {
            return tasks == null
                ? Array.Empty<Task>()
                : tasks.Where(x => x.IsCompleted && !x.IsCanceled && !x.IsFaulted);
        }

        /// <summary>
        /// Returns all successfuly completed tasks.
        /// </summary>
        public static IEnumerable<Task<T>> Successful<T>(this IEnumerable<Task<T>> tasks)
        {
            return tasks == null
                ? Array.Empty<Task<T>>()
                : tasks.Where(x => x.IsCompleted && !x.IsCanceled && !x.IsFaulted);
        }

        /// <summary>
        /// Returns all successfuly unfinished or failed tasks.
        /// </summary>
        public static IEnumerable<Task> Faulted(this IEnumerable<Task> tasks)
        {
            return tasks == null
                ? Array.Empty<Task>()
                : tasks.Where(x => x.IsCompleted && x.IsFaulted);
        }

        /// <summary>
        /// Extracts results from all successfuly completed tasks.
        /// </summary>
        public static IEnumerable<T> ExtractResults<T>(this IEnumerable<Task<T>> tasks)
        {
            return tasks == null
                ? Array.Empty<T>()
                : tasks.Successful().SelectNonNull(x => x.Result);
        }

        /// <summary>
        /// Returns tuple of collected results from successfull tasks and collected exceptions from failed ones.
        /// </summary>
        public static (T[], Exception[]) ExtractResultsAndExceptions<T>(this IEnumerable<Task<T>> tasks)
        {
            if (tasks == null)
            {
                return (Array.Empty<T>(), Array.Empty<Exception>());
            }

            var results = tasks.ExtractResults().ToArray();
            var exceptions = tasks.ExtractExceptions().ToArray();
            return (results, exceptions);
        }
    }
}
