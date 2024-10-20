#nullable disable  // just a poco

namespace Willow.Rules.Model;

/// <summary>
/// A parameter for a function or macro
/// </summary>
public class FunctionParameter
{
	/// <summary>
	/// Name for the parameter (in the UI)
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Description for the parameter
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	/// An optional units for the parameter
	/// </summary>
	public string Units { get; set; }

	/// <summary>
	/// Creates a new FunctionParameter (for deserialization)
	/// </summary>
	public FunctionParameter()
	{
	}

	/// <summary>
	/// Creates a new <see cref="FunctionParameter"/>
	/// </summary>
	public FunctionParameter(string name, string description, string units)
	{
		Name = name;
		Description = description;
		Units = units;
	}
}
