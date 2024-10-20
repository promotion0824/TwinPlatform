namespace Willow.Rules.Model;

/// <summary>
/// A numeric double field
/// </summary>
public class DoubleField : RuleUIElement
{
	/// <summary>
	/// Creates a new <see cref="DoubleField"/>
	/// </summary>
	public DoubleField(string name, string id, string units) : base(name, id, RuleUIElementType.DoubleField)
	{
		ValueDouble = 0.0; // not set yet
		Units = units;
	}

	/// <summary>
	/// Apply a value to a field
	/// </summary>
	public DoubleField With(double value, string? units = null)
	{
		return new DoubleField(this.Name, this.Id, units ?? this.Units) { ValueDouble = value };
	}

}
