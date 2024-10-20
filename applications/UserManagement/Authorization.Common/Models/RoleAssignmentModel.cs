namespace Authorization.Common.Models;

/// <summary>
/// Base class for Role Assignment Model.
/// </summary>
public abstract class RoleAssignmentModel
{
    /// <summary>
    /// Unique Identifier for the Group Role Assignment
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Assigned Role
    /// </summary>
    public RoleModel Role { get; set; } = null!;

    /// <summary>
    /// Assignment Expression.
    /// </summary>
    public string? Expression { get; set; }

    /// <summary>
    /// Condition on which the Role is assigned to the Group
    /// </summary>
    public string? Condition { get; set; }

    /// <summary>
    /// Expression Status
    /// </summary>
    public ExpressionStatus ConditionExpressionStatus { get; set; } = ExpressionStatus.Unknown;
}

