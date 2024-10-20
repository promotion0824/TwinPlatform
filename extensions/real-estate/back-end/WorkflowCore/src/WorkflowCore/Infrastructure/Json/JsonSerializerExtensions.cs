using System.Text.Json;
using System.Text.Json.Serialization;
using Willow.Infrastructure;

namespace WorkflowCore.Infrastructure.Json
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
