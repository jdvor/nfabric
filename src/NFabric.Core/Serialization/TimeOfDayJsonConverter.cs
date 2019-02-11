namespace NFabric.Core.Serialization
{
    using NFabric.Core.Extensions;
    using NFabric.Core.ValueTypes;
    using Newtonsoft.Json;
    using System;

    public class TimeOfDayJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteRawValue("null");
            }
            else
            {
                writer.WriteRawValue("\"" + ((TimeOfDay)value).ToString() + "\"");
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
                        ? (TimeOfDay?)null
                        : TimeOfDay.Empty;
                }

                return new TimeOfDay(str);
            }
            catch (Exception ex)
            {
                var msg = $"Deserialization by calling ctor {nameof(TimeOfDay)}(\"{str}\") has failed.";
                throw new JsonSerializationException(msg, ex);
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TimeOfDay) || objectType == typeof(TimeOfDay?);
        }
    }
}
