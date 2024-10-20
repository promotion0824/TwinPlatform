using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Willow.Proxy;

public static class HttpContextExtensions
{
    public static Task ProxyToDownstreamServiceAsync(this HttpContext httpContext, string serviceName, string path, bool useQueryFromCurrentRequest, params string[] headersToForward)
    {
        if (httpContext == null)
        {
            throw new ArgumentNullException(nameof(httpContext));
        }

        var clientFactory = httpContext.RequestServices.GetService<IHttpClientFactory>();
        var httpProxy = httpContext.RequestServices.GetService<IHttpProxy>();

        var targetUri = new Uri(path, UriKind.Relative);

        return httpProxy.ProxyAsync(httpContext, serviceName, targetUri, clientFactory, useQueryFromCurrentRequest, headersToForward);
    }
}
