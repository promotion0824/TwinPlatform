using System.Collections.Generic;
using Willow.Rules.Model;

namespace RulesEngine.Web;

/// <summary>
/// A named point mapping a rule variable name to a point entity Id
/// </summary>
public class NamedPointDto
{
    /// <summary>
    /// A named point mapping a rule variable name to a point entity Id
    /// </summary>
    public NamedPointDto(NamedPoint namedPoint)
    {
        Id = namedPoint.Id;                        // twinId
        VariableName = namedPoint.VariableName;
        FullName = namedPoint.FullName;
        Unit = namedPoint.Unit;
        ShortName = namedPoint.ShortName();
        ModelId = namedPoint.ModelId;
        Locations = namedPoint.Locations;
    }

    /// <summary>
    /// The Twin Id
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The name that is used grids and expression field
    /// </summary>
    public string VariableName { get; set; }

    /// <summary>
	/// The calculated unambiguous name that used in the rule expression
	/// </summary>
	public string FullName { get; set; }

    /// <summary>
    /// The units from the Twin
    /// </summary>
    public string Unit { get; set; }

    /// <summary>
    /// The units from the Twin
    /// </summary>
    public string ShortName { get; set; }

    /// <summary>
	/// Model id
	/// </summary>
	public string ModelId { get; set; }

    /// <summary>
    /// Parent chain by locatedIn and isPartOf
    /// </summary>
    /// <remarks>
    /// This is a flattened list sorted in the ascending direction.
    /// </remarks>
    public IList<TwinLocation> Locations { get; set; } = new List<TwinLocation>(0);
}
