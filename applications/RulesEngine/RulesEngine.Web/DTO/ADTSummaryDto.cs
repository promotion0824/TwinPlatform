using System;
using Willow.Rules.Model;

// EF
#nullable disable

namespace RulesEngine.Web;

/// <summary>
/// Information about the state of ADT
/// </summary>
public class ADTSummaryDto
{
    /// <summary>
    /// Constructor using <see cref="ADTSummary"/>
    /// </summary>
    public ADTSummaryDto(ADTSummary summary)
    {
        Id = summary.Id;
        AsOfDate = summary.AsOfDate;
        CustomerEnvironmentId = summary.CustomerEnvironmentId;
        ADTInstanceId = summary.ADTInstanceId;
        CountTwins = summary.CountTwins;
        CountCapabilities = summary.CountCapabilities;
        CountRelationships = summary.CountRelationships;
        CountTwinsNotInGraph = summary.CountTwinsNotInGraph;
        CountModels = summary.CountModels;
        CountModelsInUse = summary.CountModelsInUse;
    }

    /// <summary>
    /// The Id for persistence
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// When the record was created
    /// </summary>
    public DateTimeOffset AsOfDate { get; set; }

    /// <summary>
    /// The customer environment
    /// </summary>
    public string CustomerEnvironmentId { get; set; }

    /// <summary>
    /// The ADT instance
    /// </summary>
    public string ADTInstanceId { get; set; }

    /// <summary>
    /// How many twins
    /// </summary>
    public int CountTwins { get; set; }

    /// <summary>
    /// How many twins with trend Ids
    /// </summary>
    public int CountCapabilities { get; set; }

    /// <summary>
    /// How many relationships
    /// </summary>
    public int CountRelationships { get; set; }

    /// <summary>
    /// Count of twins with no relationships
    /// </summary>
    public int CountTwinsNotInGraph { get; set; }

    /// <summary>
    /// Count of models
    /// </summary>
    public int CountModels { get; set; }

    /// <summary>
    /// Count of models in use in twin
    /// </summary>
    public int CountModelsInUse { get; set; }
}
