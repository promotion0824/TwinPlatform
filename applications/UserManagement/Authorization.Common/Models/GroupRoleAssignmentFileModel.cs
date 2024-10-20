
using Authorization.Common.Abstracts;

namespace Authorization.Common.Models;

/// <summary>
/// Export Model class that map to GroupRoleAssignment Entity
/// </summary>
public class GroupRoleAssignmentFileModel: BaseFileImportModel, IGroupRoleAssignment
{
    /// <summary>
    /// Name of the Record Type
    /// </summary>
    public const string Type = "Group Role Assignment";

    /// <summary>
    /// Assigned Group
    /// </summary>
    public string GroupName { get; set; } = null!;

	/// <summary>
	/// Assigned Role
	/// </summary>
	public string RoleName { get; set; } = null!;

	/// <summary>
	/// Assignment Expression.
	/// </summary>
	public string? Expression { get; set; }

	/// <summary>
	/// Condition on which the Role is assigned to the Group
	/// </summary>
	public string? Condition { get; set; }
}
