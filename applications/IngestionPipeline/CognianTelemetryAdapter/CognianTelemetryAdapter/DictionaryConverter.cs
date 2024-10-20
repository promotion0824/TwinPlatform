namespace Willow.CognianTelemetryAdapter;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Helper class to convert a dictionary such as ScalarValue from JSON into a Dictionary.
/// </summary>
internal class DictionaryConverter : JsonConverter<Dictionary<string, object>>
{
    public override Dictionary<string, object> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token");
        }

        var dictionary = new Dictionary<string, object>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return dictionary;
            }

            string? propertyName = reader.GetString();
            reader.Read();
            object? value = ReadValue(ref reader, options);

            if (propertyName != null && value != null && !dictionary.ContainsKey(propertyName))
            {
                dictionary[propertyName] = value;
            }
        }

        throw new JsonException("Expected EndObject token");
    }

    private object? ReadValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        object? value;
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                if (reader.TryGetInt64(out long l))
                {
                    value = l;
                }
                else
                {
                    value = reader.GetDouble();
                }

                break;
            case JsonTokenType.True:
            case JsonTokenType.False:
                value = reader.GetBoolean();
                break;
            case JsonTokenType.String:
                value = reader.GetString();
                break;
            case JsonTokenType.StartArray:
                value = ReadArray(ref reader, options);
                break;
            default:
                using (var jsonDoc = JsonDocument.ParseValue(ref reader))
                {
                    value = jsonDoc.RootElement.GetRawText();
                }

                break;
        }

        return value;
    }

    private IList<object> ReadArray(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        IList<object> list = new List<object>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }

            var item = ReadValue(ref reader, options);
            if (item != null)
            {
                list.Add(item);
            }
        }

        return list;
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<string, object> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var kvp in value)
        {
            writer.WritePropertyName(kvp.Key);
            JsonSerializer.Serialize(writer, kvp.Value, options);
        }

        writer.WriteEndObject();
    }
}
