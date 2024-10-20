using Newtonsoft.Json;

namespace Willow.AzureDigitalTwins.Services.Serialization;

public class StringCollectionConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return false;
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.StartArray)
        {
            return serializer.Deserialize(reader, objectType);
        }
        return new string[] { reader.Value.ToString() };
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
