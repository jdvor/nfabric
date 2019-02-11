namespace NFabric.Core.Serialization
{
    using NFabric.Core.Extensions;
    using NFabric.Core.ValueTypes;
    using Newtonsoft.Json;
    using System;

    public class RichTokenJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteRawValue("null");
            }
            else
            {
                writer.WriteRawValue("\"" + ((RichToken)value).ToString() + "\"");
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var str = reader.Value as string;
            if (string.IsNullOrEmpty(str) || str == "null")
            {
                return objectType.IsNullable()
                    ? (RichToken?)null
                    : RichToken.Empty;
            }

            if (RichToken.TryParse(str, out RichToken token))
            {
                return token;
            }

            var msg = $"Deserialization by calling Fabric.Core.ValueTypes.RichToken.TryParse(\"{str}\") has failed.";
            throw new JsonSerializationException(msg);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(RichToken) || objectType == typeof(RichToken?);
        }
    }
}
