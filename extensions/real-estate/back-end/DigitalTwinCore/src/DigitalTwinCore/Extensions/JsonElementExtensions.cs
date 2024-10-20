using System.Collections.Generic;

namespace System.Text.Json
{
    public static class JsonElementExtensions
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
                    // Use GetDecimal for the higher precisions
                    // When in exponential format, use GetDouble instead
                    return element.TryGetDecimal(out var result) ? result : element.GetDouble();
                default:
                    return null;
            }
        }
    }
}
