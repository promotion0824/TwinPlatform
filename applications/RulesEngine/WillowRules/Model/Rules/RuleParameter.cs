using System;
using Newtonsoft.Json;
using Willow.ExpressionParser;
using Willow.Expressions;
using Willow.Expressions.Visitor;

#nullable disable  // just a poco

namespace Willow.Rules.Model;

/// <summary>
/// Cumulative types to create cumulative expressions
/// </summary>
public enum CumulativeType
{
	/// <summary>Normal Expression</summary>
	Simple = 0,
	/// <summary>The cumulative sum of values that have been added together over time.</summary>
	Accumulate = 1,
	/// <summary>The cumulative sum of values at each time point, multiplied by the corresponding time value in seconds.</summary>
	AccumulateTimeSeconds = 2,
	/// <summary>The cumulative sum of values at each time point, multiplied by the corresponding time value in minutes.</summary>
	AccumulateTimeMinutes = 3,
	/// <summary>The cumulative sum of values at each time point, multiplied by the corresponding time value in hours.</summary>
	AccumulateTimeHours = 4
}

/// <summary>
/// A <see cref="RuleTemplate"/> expects one or more Rule parameters which define the capabilities to pick off the rule instance's sub-graph
/// </summary>
public class RuleParameter
{
	/// <summary>
	/// Name for the parameter (in the UI)
	/// </summary>
	/// <example>
	/// Damper is open
	/// </example>
	[JsonProperty(Order = 0)]
	public string Name { get; set; }

	/// <summary>
	/// Description for the parameter
	/// </summary>
	/// <remarks>
	/// This is not shown in the UI, tooltip maybe?
	/// </remarks>
	/// <example>
	/// This field compares the setpoint to the actual value
	/// </example>
	[JsonProperty(Order = 1)]
	public string Description { get; set; }

	/// <summary>
	/// Field Id, the field that this parameter supplies
	/// </summary>
	[JsonProperty(Order = 2)]
	public string FieldId { get; set; }

	/// <summary>
	/// Point expression to find the right capability from the sub-graph
	/// </summary>
	/// <remarks>
	/// This uses variable names like 'Discharge Air Fan Lead Stage Manual Sp' which may based on tags initially but really
	/// we want it based on dtmi ids.
	/// </remarks>
	/// <example>
	/// Discharge Air Fan Lead Stage Manual Sp
	/// </example>
	/// <example>
	/// dtmi:com:willowinc:ExhaustAirFlowSetpoint;1
	/// </example>
	/// <example>
	/// ExhaustAirFlowSetpoint         // allow substring matches??
	/// </example>
	[JsonProperty(Order = 3)]
	public string PointExpression { get; set; }

	/// <summary>
	/// An optional unit of measure for the parameter
	/// </summary>
	/// <example>
	/// kilowattHour
	/// </example>
	/// <example>
	/// %
	/// </example>
	[JsonProperty(Order = 4)]
	public string Units { get; set; }

	/// <summary>
	/// Cumulative expressions setting applied each time a rule is evaluated, used to calculate total impact over time
	/// </summary>
	[JsonProperty(Order = 5)]
	public CumulativeType CumulativeSetting { get; set; }

	public TokenExpression GetTokenExpression()
	{
		var environment = defaultParserEnvironment.Value;
		var pointExpression = Parser.Deserialize(PointExpression, environment);
		return pointExpression;
	}

	/// <summary>
	/// Creates a new RuleParameter (for deserialization)
	/// </summary>
	public RuleParameter()
	{
	}

	/// <summary>
	/// Creates a new <see cref="RuleParameter"/>
	/// </summary>
	public RuleParameter(string name, string fieldId, string pointExpression, string units, CumulativeType cumulativeType = CumulativeType.Simple)
	{
		Name = name;
		FieldId = fieldId;
		PointExpression = pointExpression;
		Units = units;
		CumulativeSetting = cumulativeType;
	}

	/// <summary>
	/// Creates a new <see cref="RuleParameter"/>
	/// </summary>
	public RuleParameter(string name, string fieldId, string pointExpression, CumulativeType cumulativeType = CumulativeType.Simple)
		: this(name, fieldId, pointExpression, string.Empty, cumulativeType)
	{
	}

	/// <summary>
	/// Gets the default parser environment with the built-in functions like FAHRENHEIT, CELCIUS
	/// </summary>
	private static Lazy<ParserEnvironment> defaultParserEnvironment = new Lazy<ParserEnvironment>(() =>
	{
		var environment = new ParserEnvironment();
		// environment.AddVariable("pi", typeof(double));  // mostly for adding function declarations
		// Declare some conversion functions
		environment.AddFunction(RegisteredFunction.Create<Func<double>>("FAHRENHEIT"));
		environment.AddFunction(RegisteredFunction.Create<Func<double>>("CELSIUS"));
		return environment;
	});


	/// <summary>
	/// Create a <see cref="RuleParameter" />
	/// </summary>
	public static RuleParameter Create(string name, string fieldId, string pointExpression, string units = null, CumulativeType cumulativeType = CumulativeType.Simple)
	{
		try
		{
			return new RuleParameter(name, fieldId, pointExpression, units, cumulativeType);
		}
		catch (ParserException)
		{
			return new RuleParameter(name, fieldId, $"\"COULD NOT PARSE: {pointExpression}\"", cumulativeType);
		}
	}
}
