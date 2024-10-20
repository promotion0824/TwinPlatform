using Authorization.Common.Abstracts;

namespace Authorization.Common.Models;

/// <summary>
/// Permission File Model used for Permission Import Export action.
/// </summary>
public class PermissionFileModel : BaseFileImportModel, IPermission
{
    /// <summary>
    /// Name of the Record Type
    /// </summary>
    public const string Type = "Permission";

    /// <summary>
    /// Name of the Permission
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Extension name the permission belongs to
    /// </summary>
    public string Application { get; set; } = null!;

    /// <summary>
    /// Short description the permission is being used for
    /// </summary>
    public string? Description { get; set; }
}
