namespace Willow.IoTService.Deployment.Dashboard.Infrastructure;

public class AadOptions
{
    public string Instance { get; init; } = string.Empty;

    public string TenantId { get; init; } = string.Empty;

    public string ClientId { get; init; } = string.Empty;

    // ReSharper disable once CollectionNeverUpdated.Global
    // the update is used when mapping the properties
    public string[] B2CScopes { get; init; } = [];
}
