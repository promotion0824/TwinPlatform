namespace Willow.Rules;

/// <summary>
/// An indicator on an entity whether it is linked to a Willow standard rule
/// </summary>
public interface IWillowStandardRule
{
	/// <summary>
	/// Indicates a Willow standard rule
	/// </summary>
	bool IsWillowStandard { get; }
}
