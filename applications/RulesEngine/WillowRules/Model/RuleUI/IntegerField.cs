namespace Willow.Rules.Model;

/// <summary>
/// A numeric integer field
/// </summary>
public class IntegerField : RuleUIElement
{
	/// <summary>
	/// Creates a new <see cref="NumericField"/>
	/// </summary>
	public IntegerField(string name, string id, string units = "")
		: base(name, id, RuleUIElementType.IntegerField)
	{
		Units = units;
		ValueInt = 11;
	}

	/// <summary>
	/// Apply a value to a field
	/// </summary>
	public IntegerField With(int value)
	{
		return new IntegerField(this.Name, this.Id, this.Units) { ValueInt = value };
	}
}
