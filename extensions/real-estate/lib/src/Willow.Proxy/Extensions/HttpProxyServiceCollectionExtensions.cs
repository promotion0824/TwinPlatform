using Microsoft.Extensions.DependencyInjection;

namespace Willow.Proxy;

public static class HttpProxyServiceCollectionExtensions
{
    public static void AddHttpProxy(this IServiceCollection services)
    {
        services.AddScoped<IHttpProxy, HttpProxy>();
    }
}