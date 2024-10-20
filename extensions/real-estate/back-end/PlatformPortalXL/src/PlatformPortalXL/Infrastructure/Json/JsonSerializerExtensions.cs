using System.Text.Json;
using System.Text.Json.Serialization;

namespace Willow.Infrastructure
{
    public static class JsonSerializerExtensions
    {
        public static JsonSerializerOptions DefaultOptions =
        new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                new DateTimeConverter()
            }
        };
    }
}
