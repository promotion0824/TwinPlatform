
namespace Authorization.Common.Models;

/// <summary>
/// Authorization Data Import Model
/// </summary>
public class ImportModel
{
    /// <summary>
    /// Application Option payload.
    /// </summary>
    public ApplicationOption? ApplicationOption { get; set; }

    /// <summary>
    /// List of Permission
    /// </summary>
    public List<CreatePermissionModel>? Permissions { get; set; }

	/// <summary>
	/// List of Roles
	/// </summary>
	public List<CreateRoleModel>? Roles { get; set; }

    /// <summary>
    /// List of Groups
    /// </summary>
    public List<CreateGroupModel>? Groups { get; set; }

    /// <summary>
    /// List of Group Assignments
    /// </summary>
    public List<CreateGroupAssignmentModel>? GroupAssignments { get; set; }
}

/// <summary>
/// Option for Application
/// </summary>
public record ApplicationOption
{
    /// <summary>
    /// Description for the application.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Support Client Authentication.
    /// </summary>
    public bool SupportClientAuthentication { get; set; } = false;
}

/// <summary>
/// DTO class used for creating Permission Record
/// </summary>
public class CreatePermissionModel
{
	/// <summary>
	/// Name of the Permission
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Description for the Permission
	/// </summary>
	public string? Description { get; set; }
}

/// <summary>
/// Import model for Role information
/// </summary>
public class CreateRoleModel
{
	/// <summary>
	/// Name of the Role
	/// </summary>
	public string Name { get; set; }

    /// <summary>
    /// Description of the Role
    /// </summary>
    public string? Description { get; set; } = string.Empty;

    /// <summary>
    /// List of Permission names
    /// </summary>
    public string[]? Permissions { get; set; }
}

/// <summary>
/// Import model for groups
/// </summary>
public record CreateGroupModel
{
    /// <summary>
    /// Group Name
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Group Type. Allowed types Application, AzureB2C, AzureAD
    /// </summary>
    public string Type { get; set; } = null!;   
}

/// <summary>
/// Import Model for Group Assignments
/// </summary>
public record CreateGroupAssignmentModel
{
    /// <summary>
    /// Group Name for Assignment
    /// </summary>
    public string GroupName { get; set; } = null!;

    /// <summary>
    /// Role Name for Assignment
    /// </summary>
    public string RoleName { get; set; } = null!;

    /// <summary>
    /// Resource Expression.
    /// </summary>
    public string Expression { get; set; } = string.Empty;

    /// <summary>
    /// Condition.
    /// </summary>
    public string Condition { get; set; } = string.Empty;

}
