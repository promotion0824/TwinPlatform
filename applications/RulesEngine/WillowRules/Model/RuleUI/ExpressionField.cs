namespace Willow.Rules.Model;

/// <summary>
/// An expression field which is bound to variables from an environment which consists of the subgraph query elements
/// plus the global environment (weather, time of day, ...)
/// </summary>
public class ExpressionField : RuleUIElement
{
	/// <summary>
	/// Creates a new <see cref="ExpressionField"/>
	/// </summary>
	public ExpressionField(string name, string id)
		: base(name, id, RuleUIElementType.ExpressionField)
	{
		ValueString = "NOT SET";
	}

	/// <summary>
	/// Apply a value to a field
	/// </summary>
	public ExpressionField With(string value)
	{
		return new ExpressionField(this.Name, this.Id) { ValueString = value };
	}

}
