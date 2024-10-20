
using Authorization.Common.Abstracts;

namespace Authorization.Common.Models;

/// <summary>
/// Export Model class that map to UserRoleAssignment Entity
/// </summary>
public class UserRoleAssignmentFileModel : BaseFileImportModel, IUserRoleAssignment
{
    /// <summary>
    /// Name of the Record Type
    /// </summary>
    public const string Type = "User Role Assignment";

	public string UserEmail { get; set; } = null!;
	public string RoleName { get; set; } = null!;
	public string? Expression { get; set; }
	public string? Condition { get; set; }
}
