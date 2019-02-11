namespace NFabric.Core.Serialization
{
    using System;
    using System.IO;

    public interface IStreamSerialization
    {
        T Deserialize<T>(Stream stream)
            where T : class;

        object Deserialize(Stream stream, Type type = null);

        void Serialize<T>(T entity, Stream stream)
            where T : class;

        void Serialize(object entity, Stream stream, Type asType = null);

        string ContentType { get; }
    }
}
