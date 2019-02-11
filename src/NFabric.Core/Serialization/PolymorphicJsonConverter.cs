namespace NFabric.Core.Serialization
{
    using NFabric.Core.Extensions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public class PolymorphicJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(object);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                var v = JValue.Load(reader);
                return Convert(v);
            }
            catch (JsonReaderException ex)
            {
                Debug.WriteLine(ex.Describe(), "serialization");
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        private static object Convert(JToken jt)
        {
            switch (jt.Type)
            {
                case JTokenType.String:
                    return jt.ToObject<string>();

                case JTokenType.Integer:
                    return jt.ToObject<int>();

                case JTokenType.Float:
                    return jt.ToObject<double>();

                case JTokenType.Boolean:
                    return jt.ToObject<bool>();

                case JTokenType.Date:
                    return jt.ToObject<DateTimeOffset>();

                case JTokenType.Guid:
                    return jt.ToObject<Guid>();

                case JTokenType.TimeSpan:
                    return jt.ToObject<TimeSpan>();

                case JTokenType.Uri:
                    return jt.ToObject<Uri>();

                case JTokenType.Bytes:
                    return jt.ToObject<byte[]>();

                case JTokenType.Array:
                    var vals = new List<object>();
                    foreach (var innerJt in jt.Children())
                    {
                        var v = Convert(innerJt);
                        if (v != null)
                        {
                            vals.Add(v);
                        }
                    }

                    return vals;

                default:
                    return jt.ToString();
            }
        }
    }
}
