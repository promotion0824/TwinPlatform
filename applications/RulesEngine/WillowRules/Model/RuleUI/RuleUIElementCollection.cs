using System;
using System.Linq;

namespace Willow.Rules.Model;

/// <summary>
/// A collection of RuleUI elements with lookup by name and type
/// </summary>
public class RuleUIElementCollection
{
	/// <summary>
	/// RuleElements for the Typescript code to render
	/// </summary>
	/// <remarks>
	/// Public because we want to use it in Javascript
	/// </remarks>
	public RuleUIElement[] Elements { get; init; }

	/// <summary>
	/// Creates a new <see cref="RuleUIElementCollection"/>
	/// </summary>
	public RuleUIElementCollection(params RuleUIElement[] elements)
	{
		this.Elements = elements ?? throw new System.ArgumentNullException(nameof(elements));
	}

	/// <summary>
	/// Creates a new <see cref="RuleUIElementCollection"/>
	/// </summary>
	public RuleUIElementCollection() : this(Array.Empty<RuleUIElement>())
	{
	}

	private bool TryGet<T>(string id, out T? result)
	{
		result = this.Elements.Where(e => e.Id == id).OfType<T>().FirstOrDefault();
		return result is T;
	}

	/// <summary>
	/// Gets a double or percentage field, also allows fallback to an int value field
	/// </summary>
	public bool TryGetDoubleField(RuleUIElement element, out double result)
	{
		string id = element.Id;
		if (TryGet(id, out DoubleField? field)) { result = field?.ValueDouble ?? 0.0; return true; }
		if (TryGet(id, out PercentageField? fieldPercent)) { result = fieldPercent?.ValueDouble ?? 0.0; return true; }
		if (TryGetIntField(id, out int integerValue)) { result = integerValue; return true; }
		result = 0.0;
		return false;
	}

	/// <summary>
	/// Gets an integer field, will not accept a double or percentage field
	/// </summary>
	public bool TryGetIntField(string id, out int result)
	{
		bool ok = TryGet(id, out IntegerField? field);
		result = field?.ValueInt ?? 0;
		return ok;
	}

	public bool TryGetIntField(RuleUIElement element, out int result)
	{
		string id = element.Id;
		bool ok = TryGet(id, out IntegerField? field);
		result = field?.ValueInt ?? 0;
		return ok;
	}

	public bool TryGetExpressionField(string id, out string result)
	{
		bool ok = TryGet(id, out ExpressionField? field);
		result = field?.ValueString ?? "";
		return ok;
	}

	public bool TryGetExpressionField(RuleUIElement element, out string result)
	{
		string id = element.Id;
		bool ok = TryGet(id, out ExpressionField? field);
		result = field?.ValueString ?? "";
		return ok;
	}

	public bool TryGetStringField(string id, out string result)
	{
		bool ok = TryGet(id, out StringField? field);
		result = field?.ValueString ?? "";
		return ok;
	}

	public bool TryGetStringField(RuleUIElement element, out string result)
	{
		string id = element.Id;
		bool ok = TryGet(id, out StringField? field);
		result = field?.ValueString ?? "";
		return ok;
	}

	public static implicit operator RuleUIElementCollection(RuleUIElement[] array)
	{
		return new RuleUIElementCollection(array);
	}
}
