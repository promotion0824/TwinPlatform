using System.Text.Json;
using System.Threading.Tasks;
using Willow.Infrastructure;

namespace System.Net.Http
{
    public static class HttpContentExtensions
    {
        public static async Task<T> ReadAsAsync<T>(this HttpContent content)
        {
            var stream = await content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<T>(
                stream,
                JsonSerializerExtensions.DefaultOptions
            );
        }
    }
}
