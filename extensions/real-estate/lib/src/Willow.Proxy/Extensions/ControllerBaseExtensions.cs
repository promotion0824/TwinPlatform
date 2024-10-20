using Microsoft.AspNetCore.Mvc;

namespace Willow.Proxy;

public static class ControllerBaseExtensions
{
    public static Task ProxyToDownstreamService(this ControllerBase controller, string serviceName, string path, params string[] headersToForward)
    {
        if (controller == null)
        {
            throw new ArgumentNullException(nameof(controller));
        }

        return controller.HttpContext.ProxyToDownstreamServiceAsync(serviceName, path, false, headersToForward);
    }
}