using Willow.Infrastructure.MultiRegion;

namespace System.Net.Http
{
    public static class HttpResponseMessageExtensionsForMultiRegion
    {
        public static void EnsureSuccessStatusCode(
            this HttpResponseMessage message,
            string dependencyServiceName,
            string regionId
        )
        {
            var serviceName = MultiRegionHelper.ServiceName(dependencyServiceName, regionId);
            message.EnsureSuccessStatusCode(serviceName);
        }
    }
}
