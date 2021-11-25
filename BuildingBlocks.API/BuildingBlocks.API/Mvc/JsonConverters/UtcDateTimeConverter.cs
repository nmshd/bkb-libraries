using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Enmeshed.BuildingBlocks.API.Mvc.JsonConverters
{
    public class UtcDateTimeConverter : JsonConverter<DateTime?>
    {
        private readonly string _format;

        public UtcDateTimeConverter(string format = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffZ")
        {
            _format = format;
        }

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Debug.Assert(typeToConvert == typeof(DateTime));
            var stringValue = reader.GetString();
            return stringValue == null ? null : DateTime.Parse(stringValue);
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (!value.HasValue)
                writer.WriteNullValue();
            else
                writer.WriteStringValue(value.Value.ToUniversalTime().ToString(_format));
        }
    }
}