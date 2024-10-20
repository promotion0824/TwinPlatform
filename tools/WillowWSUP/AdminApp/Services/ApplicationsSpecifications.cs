namespace Willow.AdminApp;

/// <summary>
/// Manages the list of applications that are loaded in each customer instance
/// </summary>
public class ApplicationSpecification
{
    public string Name { get; }

    /// <summary>
    /// Path to access the app
    /// </summary>
    public string Path { get; }

    public string HealthEndpointPath { get; }

    /// <summary>
    /// Role name starts with for queries
    /// </summary>
    public string RoleName { get; }

    /// <summary>
    /// The container/service name.
    /// </summary>
    /// <value></value>
    public string? ServiceName { get; }

    public ApplicationSpecification(string name, string path, string healthEndpointPath,
     string roleName, string serviceName = null!)
    {
        Name = name;
        Path = path;
        HealthEndpointPath = healthEndpointPath;
        RoleName = roleName;
        ServiceName = serviceName;
    }

    public static readonly ApplicationSpecification CommandMultiTenantAU = new ApplicationSpecification("Command Australia", "https://command.willowinc.com", "https://command.willowinc.com/au/api/healthcheck", "real-estate-web");

    public static readonly ApplicationSpecification CommandMultiTenantEU = new ApplicationSpecification("Command Europe", "https://command.willowinc.com", "https://command.willowinc.com/eu/api/healthcheck", "real-estate-web");

    public static readonly ApplicationSpecification CommandMultiTenantUS = new ApplicationSpecification("Command USA", "https://command.willowinc.com", "https://command.willowinc.com/us/api/healthcheck", "real-estate-web");

    public static readonly ApplicationSpecification PublicApiMultitenant = new ApplicationSpecification("Public API", "https://command.willowinc.com/api/swagger", "https://command.willowinc.com/api/healthcheck", "PublicAPI");

    public static readonly ApplicationSpecification CommandAndControl = new ApplicationSpecification("Command and Control", "activecontrol/", "activecontrol/healthz", "CommandAndControl");
    public static readonly ApplicationSpecification NewBuild = new ApplicationSpecification("New build", "new-build-web/", "new-build-web/healthz", "NewBuild");
    public static readonly ApplicationSpecification PublicApi = new ApplicationSpecification("Public API", "publicapi/swagger", "publicapi/healthz", "PublicAPI");
    public static readonly ApplicationSpecification PortalXL = new ApplicationSpecification("Portal XL", "us/api/swagger", "us/api/healthz", "PortalXL");
}
