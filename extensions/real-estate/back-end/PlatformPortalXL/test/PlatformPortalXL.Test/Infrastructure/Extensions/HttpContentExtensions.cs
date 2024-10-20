using PlatformPortalXL.Helpers;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.ExceptionHandling;

namespace Willow.Infrastructure
{
    public static class HttpContentExtensions
    {
        public static async Task<ErrorResponse> ReadAsErrorResponseAsync(this HttpContent content)
        {
            var s = await content.ReadAsStringAsync();
            return JsonSerializerHelper.Deserialize<ErrorResponse>(s);
        }
    }
}
