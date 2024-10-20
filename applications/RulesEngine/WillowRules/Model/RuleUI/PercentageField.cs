namespace Willow.Rules.Model;

/// <summary>
/// A percentage field (0.0 - 1.0)
/// </summary>
public class PercentageField : RuleUIElement
{
	/// <summary>
	/// Creates a new <see cref="PercentageField"/>
	/// </summary>
	public PercentageField(string name, string id)
		: base(name, id, RuleUIElementType.PercentageField)
	{
		ValueDouble = 0.0; // not set yet
		Units = "%";
	}

	/// <summary>
	/// Apply a value to a field
	/// </summary>
	public PercentageField With(double value)
	{
		return new PercentageField(this.Name, this.Id) { ValueDouble = value };
	}

}
