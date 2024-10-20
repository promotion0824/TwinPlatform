
namespace Authorization.TwinPlatform.Common.Model;

/// <summary>
/// DTO class for representing permission data
/// </summary>
public class AuthorizedPermission
{
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
    /// Defines the expression that evaluates to one or more resources the current permission is applicable for.
    /// Willow Expression is recommended the Expression Language. Please refer <seealso href="https://github.com/WillowInc/TwinPlatform/tree/main/core/RulesEngine/WillowExpressions"/>
    /// </summary>
    /// <remarks>
    /// UM does not evaluate the expression from any assignments.It is up to the calling app to interpret and make sense.
    /// </remarks>
    public string? Expression { get; set; }
}

