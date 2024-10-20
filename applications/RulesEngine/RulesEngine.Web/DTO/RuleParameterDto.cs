using System;
using Newtonsoft.Json;
using Willow.Rules.Model;

// POCO class
#nullable disable

namespace RulesEngine.Web;

/// <summary>
/// Rule parameter DTO for UI
/// </summary>
public class RuleParameterDto
{
	/// <summary>
	/// Gets the name of the <see cref="RuleParameter"/>
	/// </summary>
	public string Name { get; init; }

	/// <summary>
	/// Gets the Id
	/// </summary>
	public string FieldId { get; init; }

	/// <summary>
	/// Gets the serialized expression for the <see cref="RuleParameter"/>
	/// </summary>
	public string PointExpression { get; init; }

	/// <summary>
	/// An optional unit of measure for the parameter
	/// </summary>
	public string Units { get; init; }

	/// <summary>
	/// Cumulative expressions setting applied each time a rule is evaluated, used to calculate total impact over time
	/// </summary>
	public CumulativeType CumulativeSetting { get; init; }

    /// <summary>
    /// Creates a new <see cref="RuleParameterDto"/>
    /// </summary>
    public RuleParameterDto(RuleParameter p)
    {
        Name = p.Name;
        FieldId = p.FieldId;
        PointExpression = p.PointExpression ?? "";
        Units = p.Units;
        CumulativeSetting = p.CumulativeSetting;
    }

    /// <summary>
	/// Creates a new <see cref="RuleParameterBound"/>
	/// </summary>
	public RuleParameterDto(RuleParameterBound p)
    {
        Name = p.Name;
        FieldId = p.FieldId;
        PointExpression = p.PointExpression.Serialize();
        Units = p.Units;
        CumulativeSetting = p.CumulativeSetting;
    }

    /// <summary>
    /// Constructor for deserialization
    /// </summary>
    [JsonConstructor]
	private RuleParameterDto()
	{
	}
}
