
using Authorization.Common.Models;

namespace Authorization.TwinPlatform.Permission.Api.DTO;

/// <summary>
/// Permission Response DTO Record
/// </summary>
public record PermissionResponse
{
	/// <summary>
	/// Permission Response Record Constructor
	/// </summary>
	/// <param name="model">Instance of Conditional Permission Model</param>
	public PermissionResponse(ConditionalPermissionModel model)
	{
		Name = model.Permission.Name;
		Extension = model.Permission.Application.Name;
		Description = model.Permission.Description;
		FullName = model.Permission.FullName;
		Expression = model.Expression;
		Condition = model.Condition;
	}

	///ATTENTION: Making changes to this record properties changes the contract,
	///so make sure you also update the Authorization.TwinPlatform.Common.AuthorizedPermission class
	///To match the signature of the current class

	/// <summary>
	/// Name of the Permission
	/// </summary>
	public string Name { get; set; } = null!;

	/// <summary>
	/// Extension name the permission belongs to
	/// </summary>
	public string Extension { get; set; } = null!;

	/// <summary>
	/// Short description the permission is being used for
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Full name of the Permission. Usually ExtensionName.Name
	/// </summary>
	public string? FullName { get; set; }

	/// <summary>
	/// Resource Expression the permission applicable for
	/// </summary>
	public string? Expression { get; set; }

	/// <summary>
	/// Condition applied to the Permission
	/// </summary>
	public string? Condition { get; set; }

}
