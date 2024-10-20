using System.Threading.Tasks;
using Willow.Infrastructure.MultiRegion;

namespace System.Net.Http
{
    public static class HttpResponseMessageExtensionsForMultiRegion
    {
        public static async Task EnsureSuccessStatusCode(this HttpResponseMessage message, string dependencyServiceName, string regionId)
        {
            var serviceName = MultiRegionHelper.ServiceName(dependencyServiceName, regionId);
            await message.EnsureSuccessStatusCode(serviceName);
        }
    }
}
