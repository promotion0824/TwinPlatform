using System.Collections.Generic;
using System.Text.Json;

namespace Willow.AzureDigitalTwins.Api.Extensions
{
    public static class SerializationExtensions
    {
        public static object ToObject(this JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.Array:
                    List<object> output = new List<object>();
                    foreach (var item in element.EnumerateArray())
                    {
                        output.Add(ToObject(item));
                    }
                    return output;
                case JsonValueKind.Object:
                    Dictionary<string, object> outputDictionary = new Dictionary<string, object>();
                    foreach (var item in element.EnumerateObject())
                    {
                        var value = ToObject(item.Value);
                        outputDictionary.Add(item.Name, value);
                    }
                    return outputDictionary;
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    return element.GetDecimal();
                default:
                    return null;
            }
        }

        public static T? DeserializeAnonymousType<T>(string json, T anonymousTypeObject, JsonSerializerOptions? options = default)
            => JsonSerializer.Deserialize<T>(json, options);
    }
}
