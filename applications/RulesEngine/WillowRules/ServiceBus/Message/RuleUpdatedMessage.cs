#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Willow.Rules.Model;

/// <summary>
/// Rule updated message
/// </summary>
public class RuleUpdatedMessage
{
	/// <summary>
	/// The id of the rule or "" for all rules, or a rule instance, or ...
	/// </summary>
	public string EntityId { get; init; }

	public override string ToString()
	{
		return $"EntityId {EntityId}";
	}
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
