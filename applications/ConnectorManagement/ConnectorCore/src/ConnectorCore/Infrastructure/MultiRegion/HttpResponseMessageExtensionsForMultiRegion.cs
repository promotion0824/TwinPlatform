namespace System.Net.Http
{
    using Willow.Infrastructure.MultiRegion;

    internal static class HttpResponseMessageExtensionsForMultiRegion
    {
        public static void EnsureSuccessStatusCode(this HttpResponseMessage message, string dependencyServiceName, string regionId)
        {
            var serviceName = MultiRegionHelper.ServiceName(dependencyServiceName, regionId);
            message.EnsureSuccessStatusCode(serviceName);
        }
    }
}
