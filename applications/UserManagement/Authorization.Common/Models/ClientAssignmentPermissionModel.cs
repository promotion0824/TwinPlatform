namespace Authorization.Common.Models;

/// <summary>
/// Client Assignment Permission Model.
/// </summary>
public class ClientAssignmentPermissionModel
{
    /// <summary>
    /// Id of the Client Assignment Permission.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Client Assignment Id.
    /// </summary>
    public Guid ClientAssignmentId { get; set; }

    public PermissionModel Permission { get; set; } = default!;
}
