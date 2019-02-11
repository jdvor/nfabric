namespace NFabric.Core.Extensions
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Extension methods for <see cref="Stream"/> type.
    /// </summary>
    public static class StreamExtensions
    {
        private static int? GetLength(this Stream stream)
        {
            try
            {
                return (int)stream.Length;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Reads all bytes from the stream.
        /// </summary>
        public static async Task<byte[]> ReadAllAsync(this Stream stream)
        {
            if (stream == null)
            {
                return Array.Empty<byte>();
            }

            var len = stream.GetLength();
            using (var ms = new MemoryStream((len.HasValue && len.Value > 0) ? len.Value : 4096))
            {
                await stream.CopyToAsync(ms).ConfigureAwait(false);
                await ms.FlushAsync().ConfigureAwait(false);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Reads all bytes and rewinds the stream to position 0.
        /// </summary>
        public static async Task<byte[]> ReadAllAndRewindAsync(this Stream stream)
        {
            if (stream == null)
            {
                return Array.Empty<byte>();
            }

            var bytes = await ReadAllAsync(stream).ConfigureAwait(false);
            stream.Position = 0;

            return bytes;
        }

        /// <summary>
        /// Reads stream as string and rewinds the stream to position 0.
        /// </summary>
        /// <param name="e">Text encoding to use when decoding the stream; default is <see cref="Encoding.UTF8"/>.</param>
        /// <param name="bufferSize">Buffer size; default is 4kB.</param>
        public static async Task<string> ReadAllTextAndRewindAsync(this Stream stream, Encoding e = null, int bufferSize = 4096)
        {
            if (stream == null)
            {
                return string.Empty;
            }

            string s;
            using (var reader = new StreamReader(stream, e ?? Encoding.UTF8, false, bufferSize, leaveOpen: true))
            {
                s = await reader.ReadToEndAsync().ConfigureAwait(false);
            }

            stream.Position = 0;

            return s;
        }

        /// <summary>
        /// Writes the stream to binary file and rewinds the stream to position 0.
        /// </summary>
        /// <param name="filePath"></param>
        public static async Task WriteAllAndRewindAsync(this Stream stream, string filePath)
        {
            Expect.NotNull(stream, nameof(stream));
            Expect.NotEmpty(filePath, nameof(filePath));

            using (var fs = File.OpenWrite(filePath))
            {
                await stream.CopyToAsync(fs).ConfigureAwait(false);
                await fs.FlushAsync().ConfigureAwait(false);
            }

            stream.Position = 0;
        }
    }
}
