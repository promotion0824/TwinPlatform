using Authorization.Common.Abstracts;

namespace Authorization.Common.Models;

/// <summary>
/// DTO Model class that map to RoleAssignment Entity
/// </summary>
public class UserRoleAssignmentModel : RoleAssignmentModel, IUserRoleAssignment
{
    /// <summary>
    /// User Model.
    /// </summary>
	public UserModel User { get; set; } = null!;

    /// <summary>
    /// Returns a short version of the assignment information.
    /// </summary>
    /// <returns>String.</returns>
    public string GetRowName()
    {
        return $"User: {User.FullName} | Role:{Role.Name} | Expression:{Expression} | Condition: {Condition}";
    }
}
