namespace NFabric.Core.Serialization
{
    using NFabric.Core.Extensions;
    using NFabric.Core.ValueTypes;
    using Newtonsoft.Json;
    using System;

    public class TimeGuidJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteRawValue("null");
            }
            else
            {
                writer.WriteRawValue("\"" + ((TimeGuid)value).ToString("N") + "\"");
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var str = reader.Value as string;
            try
            {
                if (string.IsNullOrEmpty(str) || str == "null")
                {
                    return objectType.IsNullable()
                        ? (TimeGuid?)null
                        : TimeGuid.Empty;
                }

                return (TimeGuid)Guid.ParseExact(str, "N");
            }
            catch (Exception ex)
            {
                var msg = $"Deserialization of \"{str}\" into {nameof(TimeGuid)} has failed.";
                throw new JsonSerializationException(msg, ex);
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TimeGuid) || objectType == typeof(TimeGuid?);
        }
    }
}
