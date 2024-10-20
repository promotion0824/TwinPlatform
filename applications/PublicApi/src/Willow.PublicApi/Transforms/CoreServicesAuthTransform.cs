namespace Willow.PublicApi.Transforms;

using Willow.Api.Authentication;
using Yarp.ReverseProxy.Transforms.Builder;

/// <summary>
/// Gets an Azure token and appends it to the proxy request.
/// </summary>
internal static class CoreServicesAuthTransform
{
    public static TransformBuilderContext AddCoreServicesAuthTransform(this TransformBuilderContext context)
    {
        context.AddRequestTransform(async transformContext =>
        {
            if ((context.Cluster?.Metadata?.TryGetValue("IsCoreService", out var isCoreService) ?? false) && isCoreService == "true")
            {
                var tokenService = transformContext.HttpContext.RequestServices.GetRequiredService<IClientCredentialTokenService>();
                var accessToken = await tokenService.GetClientCredentialTokenAsync();
                transformContext.ProxyRequest.Headers.Authorization = new("Bearer", accessToken);
            }
        });

        return context;
    }
}
