
using Authorization.Common.Abstracts;

namespace Authorization.Common.Models;

/// <summary>
/// Record to Store the Permission Model along with its Condition
/// </summary>
/// <param name="Permission">Permission Model.</param>
/// <param name="Expression">Resource Expression.</param>
/// <param name="Condition">Permission Condition.</param>
public record ConditionalPermissionModel(PermissionModel Permission, string Expression, string Condition)
{

}

/// <summary>
/// DTO class that maps to the Permission Entity
/// </summary>
public class PermissionModel : IPermission
{
	/// <summary>
	/// Unique Identifier of the Model
	/// </summary>
	public Guid Id { get; set; }

	/// <summary>
	/// Name of the Permission
	/// </summary>
	public string Name { get; set; } = null!;

	/// <summary>
	/// Application Model.
	/// </summary>
	public ApplicationModel Application { get; set; } = null!;

	/// <summary>
	/// Short description the permission is being used for
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Full name of the Permission. Usually ExtensionName.Name
	/// </summary>
	public string? FullName
	{
		get
		{
			return (string.IsNullOrWhiteSpace(Application?.Name) ? $"Unknown.{Name}": $"{Application.Name}.{Name}");
		}
	}

    /// <summary>
    /// Custom to string implementation
    /// </summary>
    /// <returns>Full Name of the permission model.</returns>
    public override string ToString()
    {
        return FullName ?? base.ToString()!;
    }
}
