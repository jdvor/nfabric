namespace NFabric.Core.Serialization
{
    using NFabric.Core.ValueTypes;
    using Newtonsoft.Json;
    using System;

    public class CoarseDurationJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteRawValue("null");
            }
            else
            {
                writer.WriteRawValue("\"" + value + "\"");
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var str = reader.Value as string;
            try
            {
                return (string.IsNullOrEmpty(str) || str == "null")
                    ? default(CoarseDuration)
                    : new CoarseDuration(str);
            }
            catch (Exception ex)
            {
                var msg = $"Deserialization by calling ctor {nameof(CoarseDuration)}(\"{str}\") has failed.";
                throw new JsonSerializationException(msg, ex);
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(CoarseDuration);
        }
    }
}
