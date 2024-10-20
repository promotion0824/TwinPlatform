using System.Collections.Generic;
using Willow.Rules.Model;

namespace RulesEngine.Web.DTO;

/// <summary>
/// Insight Batch Dto
/// </summary>
public class InsightDtoBatchDto : BatchDto<InsightDto>
{
    /// <summary>
    /// Creates a new BatchDto from a <see cref="Batch{T}"/>
    /// </summary>
    public InsightDtoBatchDto(Batch<InsightDto> batch, List<InsightImpactSummaryDto> uniqueImpactScoreNames)
        : base(batch)
    {
        ImpactScoreNames = uniqueImpactScoreNames;
    }

    /// <summary>
    /// A unique list of impact score names and fields
    /// </summary>
    public List<InsightImpactSummaryDto> ImpactScoreNames { get; init; }
}
