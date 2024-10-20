using Newtonsoft.Json;

namespace Willow.Model.Serialization;

public class StringCollectionConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return false;
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.StartArray)
        {
            return serializer.Deserialize(reader, objectType);
        }

        if (reader.Value != null)
        {
            var rv = reader.Value.ToString();

            if (!string.IsNullOrEmpty(rv))
            {
                return new string[] { rv };
            }
        }

        return Array.Empty<string>();
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
