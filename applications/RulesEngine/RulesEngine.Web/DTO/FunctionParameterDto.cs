#nullable disable  // just a poco

using Willow.Rules.Model;

namespace RulesEngine.Web;

/// <summary>
/// A parameter for a function or macro
/// </summary>
public class FunctionParameterDto
{
	/// <summary>
	/// Name for the parameter (in the UI)
	/// </summary>
	public string Name { get; init; }

	/// <summary>
	/// Description for the parameter
	/// </summary>
	public string Description { get; init; }

    /// <summary>
    /// An optional units for parameter
    /// </summary>
    public string Units { get; set; }

    /// <summary>
    /// Creates a new <see cref="FunctionParameterDto"/>
    /// </summary>
    public FunctionParameterDto(FunctionParameter parameter)
	{
		Name = parameter.Name;
		Description = parameter.Description;
        Units = parameter.Units;
    }

    /// <summary>
    /// Constructor for json
    /// </summary>
    public FunctionParameterDto()
    {
    }
}
