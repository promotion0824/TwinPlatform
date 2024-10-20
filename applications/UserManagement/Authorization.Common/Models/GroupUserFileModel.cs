namespace Authorization.Common.Models;

/// <summary>
/// Group User File Model for import export operation.
/// </summary>
public class GroupUserFileModel : BaseFileImportModel
{
    /// <summary>
    /// Name of the Record Type
    /// </summary>
    public const string Type = "GroupUser";

    public string UserEmail { get; set; } = default!;
    public string Group { get; set; } = default!;
}
