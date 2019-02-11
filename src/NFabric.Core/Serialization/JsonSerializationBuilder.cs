namespace NFabric.Core.Serialization
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    public class JsonSerializationBuilder
    {
        private JsonSerializerSettings settings;
        private readonly List<JsonConverter> converters = new List<JsonConverter>();
        private bool shortenTypeNames;
        private IEnumerable<Type> initializeTypes;
        private bool usePrettyFormat;

        public JsonSerializationBuilder Settings(JsonSerializerSettings settings)
        {
            this.settings = settings;
            return this;
        }

        public JsonSerializationBuilder With(params JsonConverter[] converters)
        {
            this.converters.AddRange(converters);
            return this;
        }

        public JsonSerializationBuilder UseShortTypeNames(params Type[] initializeTypes)
        {
            shortenTypeNames = true;
            this.initializeTypes = initializeTypes;
            return this;
        }

        public JsonSerializationBuilder UsePrettyFormat()
        {
            usePrettyFormat = true;
            return this;
        }

        public JsonSerialization Build()
        {
            var js = settings ?? DefaultSettings();

            if (usePrettyFormat)
            {
                js.Formatting = Formatting.Indented;
            }

            foreach (var converter in converters)
            {
                js.Converters.Add(converter);
            }

            if (shortenTypeNames)
            {
                js.TypeNameHandling = TypeNameHandling.All;
                js.SerializationBinder = new TypeIdSerializationBinder(initializeTypes);
            }

            return new JsonSerialization(js);
        }

        public static JsonSerializerSettings DefaultSettings()
        {
            return new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Populate,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                DateParseHandling = DateParseHandling.DateTimeOffset,
            };
        }
    }
}
