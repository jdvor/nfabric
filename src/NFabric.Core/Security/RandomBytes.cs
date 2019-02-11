namespace NFabric.Core.Security
{
    using System;
    using System.Security.Cryptography;

    public sealed class RandomBytes : IDisposable
    {
        private readonly RNGCryptoServiceProvider rng;
        private readonly object bufferSync = new object();
        private int size;
        private byte[] buffer;
        private int position;
        private bool disposed;

        public RandomBytes(int initialSize = 4096)
        {
            rng = new RNGCryptoServiceProvider();
            size = initialSize;
        }

        private void EnsureBufferReady(int requestedSize)
        {
            if (requestedSize > size)
            {
                size = requestedSize;
                PopulateBuffer();
                return;
            }

            if (position == size || buffer == null)
            {
                // Position is at the end of the buffer or the buffer has not yet been initialized.
                PopulateBuffer();
            }
        }

        private void PopulateBuffer()
        {
            buffer = new byte[size];
            rng.GetBytes(buffer);
            position = 0;
        }

        private bool Fits(int requestedSize, out int fit, out int overflow)
        {
            var diff = size - position - requestedSize;
            var fits = diff >= 0;
            fit = fits ? requestedSize : size - position;
            overflow = fits ? 0 : -diff;
            return fits;
        }

        public byte[] GetBytes(int count)
        {
            var bytes = new byte[count];
            SetBytes(bytes, 0, count);
            return bytes;
        }

        public void SetBytes(byte[] bytes)
        {
            if (bytes?.Length == 0)
            {
                return;
            }

            SetBytes(bytes, 0, bytes.Length);
        }

        public void SetBytes(byte[] bytes, int offset, int count)
        {
            lock (bufferSync)
            {
                EnsureBufferReady(count);
                if (Fits(count, out var fit, out var overflow))
                {
                    // Requested byte array length fits into to buffer given the start is at position.
                    Buffer.BlockCopy(buffer, position, bytes, offset, count);
                    position += count;
                    return;
                }

                // Requested byte array length is greater than what's remaining unused in current buffer.
                // So after the yet untouched part is copied, buffer is re-populated and the copying continues
                // for the remainder.
                Buffer.BlockCopy(buffer, position, bytes, offset, fit);
                PopulateBuffer(); // resets position to 0
                Buffer.BlockCopy(buffer, position, bytes, offset + fit, overflow);
                position += overflow;
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            rng.Dispose();
            disposed = true;
        }
    }
}
