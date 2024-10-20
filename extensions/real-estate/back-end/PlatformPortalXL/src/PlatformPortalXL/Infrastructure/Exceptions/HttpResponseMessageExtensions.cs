
using System.Threading.Tasks;
using Willow.Api.Client;


namespace System.Net.Http
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task EnsureSuccessStatusCode(this HttpResponseMessage message, string dependencyServiceName)
        {
            await RestApi.EnsureSuccessStatusCode(message, "", dependencyServiceName);
        }
    }
}
