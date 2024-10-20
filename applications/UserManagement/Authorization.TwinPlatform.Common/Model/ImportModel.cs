
namespace Authorization.TwinPlatform.Common.Model;

/// <summary>
/// Authorization Data Import Model
/// </summary>
public class ImportModel
{
	public const string APIName = "Import";

    /// <summary>
    /// Application payload.
    /// </summary>
    public ApplicationOption? ApplicationOption { get; set; }

	/// <summary>
	/// List of Permission
	/// </summary>
	public List<ImportPermission>? Permissions { get; set; }

	/// <summary>
	/// List of Roles
	/// </summary>
	public List<ImportRole>? Roles { get; set; }

    /// <summary>
    /// List of Groups
    /// </summary>
    public List<ImportGroup>? Groups { get; set; }

    /// <summary>
    /// List of Group Assignments
    /// </summary>
    public List<ImportGroupAssignment>? GroupAssignments { get; set; }
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
/// Import model for Permission information
/// </summary>
public record ImportPermission
{
	/// <summary>
	/// Name of the permission
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Description for the permission
	/// </summary>
	public string? Description { get; set; }
}

/// <summary>
/// Import model for Role information
/// </summary>
public record ImportRole
{
	/// <summary>
	/// Name of the Role
	/// </summary>
	public string Name { get; set; }

    /// <summary>
    /// Name of the Role
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// List of Permission names
    /// </summary>
    public string[] Permissions { get; set; }
}

/// <summary>
/// Import model for groups
/// </summary>
public record ImportGroup
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
public record ImportGroupAssignment
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
    /// Expression
    /// </summary>
    public string Expression { get; set; } = string.Empty;

    /// <summary>
    /// Condition
    /// </summary>
    public string Condition { get; set; } = string.Empty;

}
