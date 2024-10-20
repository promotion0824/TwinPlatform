using System.Text.Json;
using System.Text.Json.Serialization;
using Willow.Infrastructure;

namespace PlatformPortalXL.Helpers
{
    public static class JsonSerializerHelper
    {
        private static JsonSerializerOptions DefaultOptions { get; set; }

        static JsonSerializerHelper()
        {
            DefaultOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                NumberHandling = JsonNumberHandling.Strict,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            DefaultOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            DefaultOptions.Converters.Add(new DateTimeConverter());
        }

        public static T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, DefaultOptions);
        }

        public static string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, DefaultOptions);
        }
    }
}
