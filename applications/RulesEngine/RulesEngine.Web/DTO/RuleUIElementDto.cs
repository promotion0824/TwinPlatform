using Newtonsoft.Json;
using Willow.Rules.Model;

// POCO class
#nullable disable

namespace RulesEngine.Web;

/// <summary>
/// An input field in the rules UI
/// </summary>
public class RuleUIElementDto
{
	/// <summary>
	/// Id for persistence
	/// </summary>
	public string Id { get; init; }

	/// <summary>
	/// Name to display in UI
	/// </summary>
	public string Name { get; init; }

	/// <summary>
	/// Units of measure
	/// </summary>
	public string Units { get; init; }

	/// <summary>
	/// The value
	/// </summary>
	public string ValueString { get; set; }

	/// <summary>
	/// The int value
	/// </summary>
	public int ValueInt { get; set; }

	/// <summary>
	/// The double value
	/// </summary>
	public double ValueDouble { get; set; }

	/// <summary>
	/// Gets the type of the UI element (used by client-side render code)
	/// </summary>
	public RuleUIElementType ElementType { get; init; }

	/// <summary>
	/// Constructor for <see cref="RuleUIElementDto"/>
	/// </summary>
	public RuleUIElementDto(RuleUIElement ruleUIElement)
	{
		Id = ruleUIElement.Id;
		Name = ruleUIElement.Name;
		Units = ruleUIElement.Units;
		ValueString = ruleUIElement.ValueString;
		ValueInt = ruleUIElement.ValueInt;
		ValueDouble = ruleUIElement.ValueDouble;
		ElementType = ruleUIElement.ElementType;
	}

	/// <summary>
	/// Constructor for deserialization
	/// </summary>
	[JsonConstructor]
	private RuleUIElementDto()
	{
	}
}