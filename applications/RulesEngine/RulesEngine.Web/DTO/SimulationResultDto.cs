using RulesEngine.Web.DTO;
using WillowRules.DTO;

#pragma warning disable CS8618 // Nullable fields in DTO

namespace RulesEngine.Web;

/// <summary>
/// A execution simulation result
/// </summary>
public class SimulationResultDto
{
    /// <summary>
    /// An error message if any
    /// </summary>
    public string Error { get; init; }

    /// <summary>
    /// An error message if any
    /// </summary>
    public string Warning { get; init; }

    /// <summary>
    /// The insight generated by the simulation
    /// </summary>
    public InsightDto Insight { get; init; }

    /// <summary>
    /// The commands generated by the simulation
    /// </summary>
    public CommandDto[] Commands { get; init; }

    /// <summary>
    /// The rule instance linked to the simulation
    /// </summary>
    public RuleInstanceDto RuleInstance { get; init; }

    /// <summary>
    /// The timeseries data for the rule
    /// </summary>
    public TimeSeriesDataDto TimeSeriesData { get; init; }
}
