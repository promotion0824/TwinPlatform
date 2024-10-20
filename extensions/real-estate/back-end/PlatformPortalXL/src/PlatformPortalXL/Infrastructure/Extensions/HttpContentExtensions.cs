using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Willow.Infrastructure;

namespace System.Net.Http
{
    public static class HttpContentExtensions
    {
        public static async Task<T> ReadAsAsync<T>(this HttpContent content)
        {
            var stream = await content.ReadAsStreamAsync();
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                NumberHandling = JsonNumberHandling.Strict,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            options.Converters.Add(new DateTimeConverter());
            return await JsonSerializer.DeserializeAsync<T>(stream, options);
        }
    }
}
