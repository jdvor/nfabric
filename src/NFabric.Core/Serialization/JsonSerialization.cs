namespace NFabric.Core.Serialization
{
    using JetBrains.Annotations;
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Text;

    public sealed class JsonSerialization : ISerialization, IStreamSerialization
    {
        private readonly JsonSerializer serializer;
        private readonly Encoding encoding;
        private const int BufferSize = 1024;

        public string ContentType => "application/json";

        public JsonSerialization([NotNull] JsonSerializerSettings settings)
        {
            serializer = JsonSerializer.Create(settings);
            encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        }

        public JsonSerialization()
            : this(JsonSerializationBuilder.DefaultSettings())
        {
        }

        public byte[] Serialize<T>(T entity)
            where T : class
        {
            if (entity == null)
            {
                return Array.Empty<byte>();
            }

            try
            {
                using (var stream = new MemoryStream())
                using (var writer = new StreamWriter(stream, encoding, BufferSize, true))
                {
                    serializer.Serialize(writer, entity);
                    writer.Flush();
                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException($"Failed to serialize message of type {typeof(T).Name}.", ex);
            }
        }

        public byte[] Serialize(object entity, Type asType = null)
        {
            if (entity == null)
            {
                return Array.Empty<byte>();
            }

            var type = asType ?? entity.GetType();

            try
            {
                using (var stream = new MemoryStream())
                using (var writer = new StreamWriter(stream, encoding, BufferSize, true))
                {
                    serializer.Serialize(writer, entity, type);
                    writer.Flush();
                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException($"Failed to serialize message of type {type.Name}.", ex);
            }
        }

        public void Serialize<T>(T entity, Stream stream)
            where T : class
        {
            if (entity == null)
            {
                return;
            }

            try
            {
                using (var writer = new StreamWriter(stream, encoding, BufferSize, true))
                {
                    serializer.Serialize(writer, entity);
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException($"Failed to serialize message of type {typeof(T).Name}.", ex);
            }
        }

        public void Serialize(object entity, Stream stream, Type asType = null)
        {
            if (entity == null)
            {
                return;
            }

            var type = asType ?? entity.GetType();

            try
            {
                using (var writer = new StreamWriter(stream, encoding, BufferSize, true))
                {
                    serializer.Serialize(writer, entity, type);
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException($"Failed to serialize message of type {type.Name}.", ex);
            }
        }

        public T Deserialize<T>(byte[] payload)
            where T : class
        {
            if (payload == null || payload.Length == 0)
            {
                return default(T);
            }

            try
            {
                using (var stream = new MemoryStream(payload))
                using (var sr = new StreamReader(stream, encoding, false, BufferSize, true))
                using (var reader = new JsonTextReader(sr))
                {
                    return serializer.Deserialize<T>(reader);
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException($"Failed to deserialize message of type {typeof(T).Name}.", ex);
            }
        }

        public T Deserialize<T>(Stream stream)
            where T : class
        {
            if (stream == null)
            {
                return default(T);
            }

            try
            {
                using (var sr = new StreamReader(stream, encoding, false, BufferSize, true))
                using (var reader = new JsonTextReader(sr))
                {
                    return serializer.Deserialize<T>(reader);
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException($"Failed to deserialize message of type {typeof(T).Name}.", ex);
            }
        }

        public object Deserialize(byte[] payload, Type type = null)
        {
            if (payload == null || payload.Length == 0)
            {
                return default(object);
            }

            try
            {
                using (var stream = new MemoryStream(payload))
                using (var sr = new StreamReader(stream, encoding, false, BufferSize, true))
                using (var reader = new JsonTextReader(sr))
                {
                    return type != null
                        ? serializer.Deserialize(reader, type)
                        : serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException($"Failed to deserialize message of type {type.Name}.", ex);
            }
        }

        public object Deserialize(Stream stream, Type type = null)
        {
            if (stream == null)
            {
                return default(object);
            }

            try
            {
                using (var sr = new StreamReader(stream, encoding, false, BufferSize, true))
                using (var reader = new JsonTextReader(sr))
                {
                    return type != null
                        ? serializer.Deserialize(reader, type)
                        : serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException($"Failed to deserialize message of type {type.Name}.", ex);
            }
        }
    }
}
