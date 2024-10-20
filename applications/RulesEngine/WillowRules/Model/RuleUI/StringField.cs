namespace Willow.Rules.Model;

/// <summary>
/// A string field
/// </summary>
public class StringField : RuleUIElement
{
	/// <summary>
	/// Creates a new <see cref="StringField"/>
	/// </summary>
	public StringField(string name, string id) : base(name, id, RuleUIElementType.StringField)
	{
		ValueString = "";
	}

	/// <summary>
	/// Apply a value to a field
	/// </summary>
	public StringField With(string value)
	{
		return new StringField(this.Name, this.Id) { ValueString = value };
	}

}
