using Authorization.Common.Abstracts;

namespace Authorization.Common.Models;

/// <summary>
/// Group File Model for import action.
/// </summary>
public class GroupFileModel : BaseFileImportModel, IGroup
{
    /// <summary>
    /// Name of the Record Type
    /// </summary>
    public const string Type = "Group";


    /// <summary>
    /// Name of the Group
    /// </summary>
    public string Name { get; set; } = null!;


    /// <summary>
    /// Group Type Name
    /// </summary>
    public string GroupType { get; set; } = null!;
}
