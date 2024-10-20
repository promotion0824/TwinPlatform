using System.ComponentModel.DataAnnotations;

namespace Authorization.Common.Models;

/// <summary>
/// Client Assignment Model
/// </summary>
public class ClientAssignmentModel
{
    /// <summary>
    /// Id of the Client Assignment.
    /// </summary>
    public Guid Id { get; set; }

    public ApplicationClientModel ApplicationClient { get; set; } = default!;

    /// <summary>
    /// Expression string that defines the scope of resources.
    /// </summary>
    [StringLength(1000)]
    public string? Expression { get; set; }

    /// <summary>
    /// Boolean condition that dynamically sets the state of the assignment.
    /// Expression supports Willow Expression Language.
    /// String.Empty or null will be considered as an active assignment.
    /// </summary>
    [StringLength(400)]
    public string? Condition { get; set; }

    /// <summary>
    /// Expression Status
    /// </summary>
    public ExpressionStatus ConditionExpressionStatus { get; set; } = ExpressionStatus.Unknown;

    public List<PermissionModel>? Permissions { get; set; }
}
