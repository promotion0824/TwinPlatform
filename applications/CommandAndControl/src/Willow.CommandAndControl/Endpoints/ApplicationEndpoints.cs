namespace Willow.CommandAndControl.Endpoints;

using Willow.CommandAndControl.Application.Filters;
using Willow.CommandAndControl.Application.Requests.ContactUs.PostContactUs;
using Willow.CommandAndControl.Application.Requests.GetUserPermissions;
using Willow.CommandAndControl.Application.Requests.ResolvedCommand.GetResolvedCommands;
using Willow.CommandAndControl.Application.Requests.Sites;

internal static class ApplicationEndpoints
{
    private const string CanViewRequestsCommandsPermission = "CanViewRequestsCommands";
    private const string CanApproveExecutePermission = "CanApproveExecute";

    public static RouteGroupBuilder MapApplicationEndpoints(this RouteGroupBuilder group)
    {
        group.MapGroup("/config")
            .WithOpenApi()
            .WithTags("Config")
            .MapConfigEndpoints();

        group.MapGroup("/requested-commands")
            .WithOpenApi()
            .WithTags("Requested Commands")
            .MapRequestedCommandsEndpoints();

        group.MapGroup("/resolved-commands")
            .WithOpenApi()
            .WithTags("Resolved Commands")
            .MapResolvedCommandsEndpoints();

        group.MapGroup("/activity-logs")
            .WithOpenApi()
            .WithTags("Activity Logs")
            .MapActivityLogEndpoints();

        group.MapGroup("/statistics")
            .WithOpenApi()
            .WithTags("Statistics")
            .MapStatisticsEndpoints();

        group.MapGroup("/sites")
            .WithOpenApi()
            .WithTags("Sites")
            .MapSitesEndpoints();

        group.MapGroup("/user")
            .WithOpenApi()
            .WithTags("User")
            .MapUserEndpoints();

        group.MapGroup("/contact-us")
            .WithOpenApi()
            .WithTags("Contact")
            .MapContactUsEndpoints();

        return group;
    }

    private static void MapConfigEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", (IOptions<ADB2COptions> azureAppOptions) =>
        {
            return Results.Ok(new
            {
                AzureAppOptions = azureAppOptions.Value,
                Version = Assembly.GetExecutingAssembly().GetName().Version!.ToString(),
            });
        })
            .WithName("get-config")
            .WithDescription("Get the application configuration")
            .WithSummary("Get the application configuration");
    }

    private static void MapRequestedCommandsEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/conflicts", GetConflictingCommandsHandler.HandleAsync)
            .WithName("get-requested-commands")
            .WithDescription("Get all conflicting requested commands")
            .WithSummary("Get all conflicting requested commands")
            .RequireAuthorization(CanViewRequestsCommandsPermission);

        group.MapPost("/conflicts/{connectorId}/{twinId}", GetConflictingCommandsByTwinIdHandler.HandleAsync)
            .WithName("get-requested-commands-by-twinid")
            .WithDescription("Get conflicting requested commands by Twin ID")
            .WithSummary("Get conflicting requested commands by Twin ID")
            .RequireAuthorization(CanViewRequestsCommandsPermission);

        group.MapGet("/{id}", GetRequestedCommandByIdHandler.HandleAsync)
            .WithName("get-requested-command-by-id")
            .WithDescription("Get a requested command by ID")
            .WithSummary("Get a requested command by ID")
            .RequireAuthorization(CanViewRequestsCommandsPermission);

        group.MapPost("/present-values", GetConflictingCommandPresentValuesHandler.HandleAsync)
            .WithName("get-requested-commands-present-values")
            .WithDescription("Get present values for requested commands by externalIds")
            .WithSummary("Get present values for requested commands by externalIds ")
            .RequireAuthorization(CanViewRequestsCommandsPermission);

        group.MapPost("/status", UpdateRequestedCommandStatusHandler.HandleAsync)
            .WithName("update-requested-command-status")
            .WithDescription("Update the status of a single requested command")
            .WithSummary("Update the status of a single requested command")
            .RequireAuthorization(CanApproveExecutePermission);

        group.MapPost("/statuses", UpdateRequestedCommandsStatusHandler.HandleAsync)
            .WithName("update-requested-commands-status")
            .WithDescription("Update the status of many requested commands")
            .WithSummary("Update the status of many requested commands")
            .RequireAuthorization(CanApproveExecutePermission);

        //The authorization used here is Azure AD since this is machine to machine call from Rules Engine to C&C
        group.MapPost("/", PostRequestedCommandsHandler.HandleAsync)
            .WithName("create-requested-commands")
            .WithDescription("Create a requested command")
            .WithSummary("Create a requested command")
            .AddEndpointFilter<ValidationFilter<PostRequestedCommandsDto>>()
            .RequireAuthorization();

        group.MapPost("/count", GetRequestedCommandsCountHandler.HandleAsync)
            .WithName("get-requested-commands-count")
            .WithDescription("Get a count of all conflicting requested commands")
            .WithSummary("Get a count of all conflicting requested commands")
            .RequireAuthorization(CanViewRequestsCommandsPermission);
    }

    private static void MapResolvedCommandsEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/", GetResolvedCommandsRequestHandler.HandleAsync)
            .WithName("get-resolved-commands")
            .WithDescription("Get all resolved commands")
            .WithSummary("Get all resolved commands")
            .RequireAuthorization(CanViewRequestsCommandsPermission);

        group.MapGet("/{id}", GetResolvedCommandByIdHandler.HandleAsync)
            .WithName("get-resolved-command-by-id")
            .WithDescription("Get a resolved command by ID")
            .WithSummary("Get a resolved command by ID")
            .RequireAuthorization(CanViewRequestsCommandsPermission);

        group.MapPost("/{id}/status", UpdateResolvedCommandStatusHandler.HandleAsync)
            .WithName("update-resolved-command-status")
            .WithDescription("Update the status of a resolved command")
            .WithSummary("Update the status of a resolved command")
            .RequireAuthorization(CanApproveExecutePermission);
    }

    public static void MapActivityLogEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost(string.Empty, GetActivityLogsHandler.HandleAsync)
            .RequireAuthorization(CanViewRequestsCommandsPermission)
            .WithDescription("Get activity logs")
            .WithSummary("Get activity logs");

        group.MapPost("export/{format?}", DownloadActivityLogsHandler.HandleAsync)
            .WithName("export-activity-logs")
            .WithDescription("Export activity logs")
            .WithSummary("Export activity logs")
            .Produces<FileStreamHttpResult>()
            .RequireAuthorization(CanViewRequestsCommandsPermission);
    }

    private static void MapStatisticsEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/", GetStatisticsHandler.HandleAsync)
            .WithName("get-statistics")
            .WithDescription("Get statistics")
            .WithSummary("Get statistics")
            .RequireAuthorization(CanViewRequestsCommandsPermission);
    }

    private static void MapSitesEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllSitesHandler.HandleAsync)
            .WithName("get-all-sites")
            .WithDescription("Get all sites")
            .WithSummary("Get all sites")
            .RequireAuthorization(CanViewRequestsCommandsPermission);
    }

    private static void MapUserEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/permissions", GetUserPermissionsHandler.HandleAsync)
            .WithName("get-user-permissions")
            .WithDescription("Get user's permissions")
            .WithSummary("Get user's permissions");
    }

    private static void MapContactUsEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/", PostContactUsHandler.HandleAsync)
            .WithName("contact-us")
            .WithDescription("Contact us")
            .WithSummary("Contact us");
    }
}
