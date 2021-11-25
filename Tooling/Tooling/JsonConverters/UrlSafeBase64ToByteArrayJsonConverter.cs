using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using gfoidl.Base64;

namespace Enmeshed.Tooling.JsonConverters
{
    public class UrlSafeBase64ToByteArrayJsonConverter : JsonConverter<byte[]>
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(byte[]);
        }

        public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var stringValue = reader.GetString();

            try
            {
                return Base64.Url.Decode(stringValue);
            }
            catch (FormatException e)
            {
                throw new JsonException(e.Message);
            }
        }

        public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
        {
            var stringValue = Base64.Url.Encode(value);
            writer.WriteStringValue(stringValue);
        }
    }
}