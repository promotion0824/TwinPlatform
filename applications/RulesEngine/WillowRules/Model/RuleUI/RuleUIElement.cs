using Newtonsoft.Json;

namespace Willow.Rules.Model;

/// <summary>
/// An input field in the rules UI
/// </summary>
/// <remarks>
/// We need some light structural typing so we can persist parameters to the database,
/// evolve them over time, show them in javascript UI, ...
/// 
/// Also, just fields right now but this could split to include other UI elements later
/// </remarks>
public abstract class RuleUIElement
{
	/// <summary>
	/// Id for persistence
	/// </summary>
	[JsonProperty(Order = 0)]
	public string Id { get; init; }

	/// <summary>
	/// Name to display in UI
	/// </summary>
	[JsonProperty(Order = 1)]
	public string Name { get; init; }

	/// <summary>
	/// Units of measure
	/// </summary>
	[JsonProperty(Order = 2)]
	public string Units { get; init; }

	// Union type for the JSON, IConvertible and JSON.net don't get along together

	/// <summary>
	/// The value
	/// </summary>
	[JsonProperty(Order = 3)]
	public string ValueString { get; set; }

	/// <summary>
	/// The int value
	/// </summary>
	[JsonProperty(Order = 4)]
	public int ValueInt { get; set; }

	/// <summary>
	/// The double value
	/// </summary>
	[JsonProperty(Order = 5)]
	public double ValueDouble { get; set; }

	/// <summary>
	/// Gets the type of the UI element (used by client-side render code)
	/// </summary>
	[JsonProperty(Order = 6)]
	public RuleUIElementType ElementType { get; init; }

	/// <summary>
	/// A method used by newtonsoft to determine whether the "ValueString" property should be serialized
	/// </summary>
	public bool ShouldSerializeValueString()
	{
		return ElementType == RuleUIElementType.StringField;
	}

	/// <summary>
	/// A method used by newtonsoft to determine whether the "ValueInt" property should be serialized
	/// </summary>
	public bool ShouldSerializeValueInt()
	{
		return ElementType == RuleUIElementType.IntegerField;
	}

	/// <summary>
	/// A method used by newtonsoft to determine whether the "ValueDouble" property should be serialized
	/// </summary>
	public bool ShouldSerializeValueDouble()
	{
		return ElementType == RuleUIElementType.DoubleField || ElementType == RuleUIElementType.PercentageField;
	}

	/// <summary>
	/// Creates a new <see cref="RuleUIElement"/>
	/// </summary>
	protected RuleUIElement(string name, string id, RuleUIElementType elementType)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new System.ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));
		}

		if (string.IsNullOrWhiteSpace(id))
		{
			throw new System.ArgumentException($"'{nameof(id)}' cannot be null or whitespace.", nameof(id));
		}

		Name = name;
		Id = id;
		ElementType = elementType;
		ValueString = "NOT SET";
		ValueDouble = 0.0;
		ValueInt = 0;
		Units = "";
	}

	public RuleUIElement()
	{
		Id = "NOT SET";
		Name = "NOT SET";
		Units = "";
		ValueString = "";
	}
}

/// <summary>
/// Element type for the web client using javascript types
/// </summary>
public enum RuleUIElementType
{
	/// <summary>
	/// A field for a double
	/// </summary>
	DoubleField = 1,

	/// <summary>
	/// A field for a percentage
	/// </summary>
	PercentageField = 2,

	/// <summary>
	/// A field for an integer
	/// </summary>
	IntegerField = 3,

	/// <summary>
	/// A field for a string
	/// </summary>
	StringField = 4,

	/// <summary>
	/// A field for a binding expression / calculation
	/// </summary>
	ExpressionField = 5
}
