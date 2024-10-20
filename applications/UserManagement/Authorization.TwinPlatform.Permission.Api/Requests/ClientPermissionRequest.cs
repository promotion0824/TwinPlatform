namespace Authorization.TwinPlatform.Permission.Api.Requests;

/// <summary>
/// Client Permission Request.
/// </summary>
public class ClientPermissionRequest
{
    /// <summary>
    /// Id of the client.
    /// </summary>
    public string ClientId { get; set; } = default!;

    /// <summary>
    /// Name of the Application.
    /// </summary>
    public string Application { get; set; } = default!;

    public string GetUniqueKey()
    {
        return $"{Application}_{ClientId}";
    }
}
