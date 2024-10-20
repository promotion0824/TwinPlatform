using Willow.Infrastructure.MultiRegion;

namespace System.Net.Http
{
    public static class HttpClientFactoryExtensionsForMultiRegion
    {
        public static HttpClient CreateClient(
            this IHttpClientFactory httpClientFactory,
            string name,
            string regionId
        )
        {
            var serviceName = MultiRegionHelper.ServiceName(name, regionId);
            return httpClientFactory.CreateClient(serviceName);
        }
    }
}
