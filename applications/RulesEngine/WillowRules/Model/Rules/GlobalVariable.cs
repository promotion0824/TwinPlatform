#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Newtonsoft.Json;
using System.Collections.Generic;
using Willow.Rules.Repository;

// POCO class
#nullable disable

namespace Willow.Rules.Model;

public enum GlobalVariableType
{
	Macro,
	Function
}

/// <summary>
/// A global variable that can be used in any rule
/// </summary>
/// <remarks>
/// These can be constants like 10c/kwH or calculated based on other variables
/// e.g. HOUR > 8 AND HOUR < 17
/// 
/// The following built-in global variables are always available:
/// 
///    HOUR       = Current hour
///    MINUTE     = Current minute
///    DAYOFWEEK  = 0 - 6 Sun-Sat
///    DAY        = 1-31
///    MONTH      = 1-12
///    YEAR       = 2021...
/// 
/// </remarks>
public class GlobalVariable : IId, IWillowStandardRule
{
	/// <summary>
	/// The ID of the global variable
	/// </summary>
	[JsonProperty("id", Order = 0)]
	public string Id { get; init; }

	/// <summary>
	/// Name of the global variable
	/// </summary>
	[JsonProperty(Order = 1)]
	public string Name { get; set; }

	/// <summary>
	/// The expressions that this global uses
	/// </summary>
	[JsonProperty(Order = 2)]
	public virtual IList<RuleParameter> Expression { get; set; } = new List<RuleParameter>();

	/// <summary>
	/// An optional units for the return value
	/// </summary>
	[JsonProperty(Order = 3)]
	public string Units { get; set; }

	/// <summary>
	/// Description
	/// </summary>
	[JsonProperty(Order = 4)]
	public string Description { get; set; }

	/// <summary>
	/// Is this global variable one of the built-in variables? If so, no Expression to edit
	/// just a description.
	/// </summary>
	[JsonProperty(Order = 5)]
	public bool IsBuiltIn { get; init; }

	/// <summary>
	/// The type of globabl variable, eg Variable, Macro or Function
	/// </summary>
	[JsonProperty(Order = 6)]
	public GlobalVariableType VariableType { get; init; }

	/// <summary>
	/// The parameters for macros and functions
	/// </summary>
	[JsonProperty(Order = 7)]
	public virtual IList<FunctionParameter> Parameters { get; set; } = new List<FunctionParameter>();

	/// <summary>
	/// Tags
	/// </summary>
	[JsonProperty(Order = 8)]
	public virtual IList<string> Tags { get; set; }

	/// <summary>
	/// Indicator whether a standard rule for willow
	/// </summary>
	[JsonProperty(Order = 9)]
	public bool IsWillowStandard { get; set; }

	public override string ToString()
	{
		return JsonConvert.SerializeObject(this);
	}
}
