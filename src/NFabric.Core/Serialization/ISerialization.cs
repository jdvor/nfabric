namespace NFabric.Core.Serialization
{
    using System;

    public interface ISerialization
    {
        T Deserialize<T>(byte[] payload)
            where T : class;

        object Deserialize(byte[] payload, Type type = null);

        byte[] Serialize<T>(T entity)
            where T : class;

        byte[] Serialize(object entity, Type asType = null);

        string ContentType { get; }
    }
}
