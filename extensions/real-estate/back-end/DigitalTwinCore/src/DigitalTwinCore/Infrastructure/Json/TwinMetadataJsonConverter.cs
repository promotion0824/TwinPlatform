using Azure.DigitalTwins.Core;
using DigitalTwinCore.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using DigitalTwinCore.Constants;
using DigitalTwinCore.Models;

namespace DigitalTwinCore.Infrastructure.Json
{
    public class TwinMetadataJsonConverter : JsonConverter<TwinMetadataDto>
    {
        public override TwinMetadataDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"Unexpected token type {reader.TokenType} at index {reader.TokenStartIndex}. Expected JsonTokenType.StartObject.");
            }

            reader.Read(); 
            var metadata = new TwinMetadataDto();

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                string propertyName = reader.GetString();

                reader.Read();

                if (propertyName == Properties.ModelId)
                {
                    metadata.ModelId = JsonSerializer.Deserialize<string>(ref reader, options);
                }
                else if (reader.TokenType == JsonTokenType.StartObject)
                {
                    metadata.WriteableProperties[propertyName] = JsonSerializer.Deserialize<TwinPropertyMetadata>(ref reader, options);
                }
                else
                {
                    throw new JsonException();
                }

                reader.Read();
            }

            return metadata;
        }

        public override void Write(Utf8JsonWriter writer, TwinMetadataDto value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString(Properties.ModelId, value.ModelId);
            foreach (KeyValuePair<string, TwinPropertyMetadata> p in value.WriteableProperties ?? Enumerable.Empty<KeyValuePair<string, TwinPropertyMetadata>>())
            {
                writer.WritePropertyName(p.Key);
                JsonSerializer.Serialize(writer, p.Value, options);
            }
            writer.WriteEndObject();
        }
    }
}
