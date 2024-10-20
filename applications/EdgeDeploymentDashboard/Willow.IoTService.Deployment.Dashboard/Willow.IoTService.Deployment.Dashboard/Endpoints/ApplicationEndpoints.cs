namespace Willow.IoTService.Deployment.Dashboard.Endpoints;

using System.Reflection;
using Microsoft.Extensions.Options;
using Willow.IoTService.Deployment.Dashboard.Options;

internal static class ApplicationEndpoints
{
    private const string CanReadPermission = "CanRead";
    private const string CanWritePermission = "CanWrite";

    public static RouteGroupBuilder MapV1ApplicationEndpoints(this RouteGroupBuilder group)
    {
        group.MapGroup("/config")
             .WithOpenApi()
             .WithTags("Config")
             .MapConfigEndpoints();

        group.MapGroup("/Deployments")
             .WithOpenApi()
             .WithTags("Deployments")
             .MapDeploymentsEndpoints();

        group.MapGroup("/ModuleTypes")
             .WithOpenApi()
             .WithTags("ModuleTypes")
             .MapModuleTypesEndpoints();

        group.MapGroup("/Modules")
             .WithOpenApi()
             .WithTags("Modules")
             .MapModulesEndpoints();

        return group;
    }

    private static void MapConfigEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet(
                     "/",
                     (IOptions<ADB2COptions> azureAppOptions) =>
                     {
                         return Results.Ok(
                                           new
                                           {
                                               AzureAppOptions = azureAppOptions.Value,
                                               Version = Assembly.GetExecutingAssembly().GetName().Version!.ToString(),
                                           });
                     }).WithSummary("Get application config");
    }

    private static void MapDeploymentsEndpoints(this RouteGroupBuilder group)
    {
    }

    private static void MapModuleTypesEndpoints(this RouteGroupBuilder group)
    {
    }

    public static void MapModulesEndpoints(this RouteGroupBuilder group)
    {
    }
}
