using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Willow.ExceptionHandling;
using WorkflowCore.Infrastructure.Json;

namespace Willow.Infrastructure
{
    public static class HttpContentExtensions
    {
        public static async Task<ErrorResponse> ReadAsErrorResponseAsync(this HttpContent content)
        {
            var s = await content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ErrorResponse>(s, JsonSerializerExtensions.DefaultOptions);
        }
    }
}
