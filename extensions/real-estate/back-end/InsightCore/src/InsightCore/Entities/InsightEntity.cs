using InsightCore.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json;
using Willow.Infrastructure;

namespace InsightCore.Entities;

[Table("Insights")]
public class InsightEntity
{
	public Guid Id { get; set; }
	public Guid CustomerId { get; set; }
	public Guid SiteId { get; set; }

	[Required(AllowEmptyStrings = false)]
	[MaxLength(64)]
	public string SequenceNumber { get; set; }
	public Guid? EquipmentId { get; set; }
	[MaxLength(250)]
	public string TwinId { get; set; }
    public string TwinName { get; set; }
	public InsightType Type { get; set; }

	[Required(AllowEmptyStrings = false)]
	[MaxLength(512)]
	public string Name { get; set; }

	[Required(AllowEmptyStrings = true)]
	[MaxLength(2048)]
	public string Description { get; set; }
	public string Recommendation { get; set; }
	public int Priority { get; set; }
	public InsightStatus Status { get; set; }
	public InsightState State { get; set; }
	public DateTime CreatedDate { get; set; }
	public DateTime UpdatedDate { get; set; }
	public DateTime LastOccurredDate { get; set; }
	public DateTime DetectedDate { get; set; }
	public SourceType SourceType { get; set; }
	public Guid? SourceId { get; set; }
	public int OccurrenceCount { get; set; }

	[Required(AllowEmptyStrings = true)]
	[MaxLength(128)]
	public string ExternalId { get; set; }

	[Required(AllowEmptyStrings = true)]
	[MaxLength(64)]
	public string ExternalStatus { get; set; }

	[Required(AllowEmptyStrings = true)]
	public string ExternalMetadata { get; set; }

	[InverseProperty(nameof(ImpactScoreEntity.Insight))]
	public List<ImpactScoreEntity> ImpactScores { get; set; } 

	public Guid? CreatedUserId { get; set; }

	[MaxLength(450)]
	public string RuleId { get; set; }
	public string RuleName { get; set; }

	public string PrimaryModelId { get; set; }
	public bool NewOccurrence { get; set; }
    public string PointsJson { get; set; }
    public bool Reported { get; set; }
    public List<string> Tags { get; set; }
    public ICollection<InsightOccurrenceEntity> InsightOccurrences { get; set; }
	public ICollection<StatusLogEntity> StatusLogs { get; set; }

    [InverseProperty(nameof(DependencyEntity.FromInsight))]
    public ICollection<DependencyEntity> Dependencies { get; set; }
    public ICollection<InsightLocationEntity> Locations { get; set; }

    public static Insight MapTo(InsightEntity entity)
	{
		if (entity == null)
		{
			return null;
		}

		return new Insight
		{
			Id = entity.Id,
			CustomerId = entity.CustomerId,
			SiteId = entity.SiteId,
			SequenceNumber = entity.SequenceNumber,
			EquipmentId = entity.EquipmentId,
			TwinId = entity.TwinId,
            TwinName = entity.TwinName,
			Type = entity.Type,
			Name = entity.Name,
			Description = entity.Description,
			Recommendation = entity.Recommendation,
			ImpactScores = ImpactScoreEntity.MapTo(entity.ImpactScores),
			Priority = entity.Priority,
			Status = entity.Status,
			State = entity.State,
			CreatedDate = entity.CreatedDate,
			UpdatedDate = entity.UpdatedDate,
			LastOccurredDate = entity.LastOccurredDate,
			DetectedDate = entity.DetectedDate,
			SourceType = entity.SourceType,
			SourceId = entity.SourceId,
			ExternalId = entity.ExternalId,
			ExternalStatus = entity.ExternalStatus,
			ExternalMetadata = entity.ExternalMetadata,
			OccurrenceCount = entity.OccurrenceCount,
			CreatedUserId = entity.CreatedUserId,
			RuleId = entity.RuleId,
			RuleName = entity.RuleName,
			PrimaryModelId = entity.PrimaryModelId,
			NewOccurrence = entity.NewOccurrence,
			InsightOccurrences = InsightOccurrenceEntity.MapTo(entity.InsightOccurrences),
			PreviouslyIgnored = PreviousStatusCount(entity,InsightStatus.Ignored),
			PreviouslyResolved = PreviousStatusCount(entity, InsightStatus.Resolved),
			Dependencies = DependencyEntity.MapTo(entity.Dependencies),
            LastIgnoredDate = (entity.StatusLogs != null && entity.StatusLogs.Any(c => c.Status == InsightStatus.Ignored)) ? (DateTime?)entity.StatusLogs.Where(c => c.Status == InsightStatus.Ignored).Max(c => c.CreatedDateTime) : null,
            LastResolvedDate = (entity.StatusLogs != null && entity.StatusLogs.Any(c => c.Status == InsightStatus.Resolved)) ? (DateTime?)entity.StatusLogs.Where(c => c.Status == InsightStatus.Resolved).Max(c => c.CreatedDateTime) : null,
            Points = MapInsightPoints(entity.PointsJson),
            Reported = entity.Reported,
            Locations = entity.Locations?.Select(l => l.LocationId).ToList(),
            Tags = entity.Tags
        };
	}

    public static List<Insight> MapTo(IEnumerable<InsightEntity> entities)
	{
		return entities?.Select(MapTo).ToList();
	}
    public static List<InsightEntity> MapFrom(IEnumerable<Insight> insights)
	{
		return insights?.Select(MapFrom).ToList();
	}
	public static InsightEntity MapFrom(Insight insight)
	{
		if (insight == null)
		{
			return null;
		}

		return new InsightEntity
		{
			Id = insight.Id,
			CustomerId = insight.CustomerId,
			SiteId = insight.SiteId,
			SequenceNumber = insight.SequenceNumber,
			EquipmentId = insight.EquipmentId,
			TwinId = insight.TwinId,
            TwinName = insight.TwinName,
			Type = insight.Type,
			Name = insight.Name,
			Description = insight.Description ?? string.Empty,
			Recommendation = insight.Recommendation,
			ImpactScores = ImpactScoreEntity.MapFrom(insight),
			Priority = insight.Priority,
			Status = insight.Status,
			State = insight.State,
			CreatedDate = insight.CreatedDate,
			LastOccurredDate = insight.LastOccurredDate,
			DetectedDate = insight.DetectedDate,
			ExternalId = insight.ExternalId ?? string.Empty,
			ExternalStatus = insight.ExternalStatus ?? string.Empty,
			ExternalMetadata = insight.ExternalMetadata ?? string.Empty,
			SourceType = insight.SourceType,
			SourceId = insight.SourceId,
			UpdatedDate = insight.UpdatedDate,
			OccurrenceCount = insight.OccurrenceCount,
			CreatedUserId = insight.CreatedUserId,
			RuleId = insight.RuleId,
			RuleName = insight.RuleName,
			PrimaryModelId = insight.PrimaryModelId,
			NewOccurrence = insight.NewOccurrence,
			InsightOccurrences = InsightOccurrenceEntity.MapFrom(insight),
			Dependencies = DependencyEntity.MapFrom(insight),
            PointsJson = insight.Points is not null ? JsonSerializer.Serialize(insight.Points, JsonSerializerExtensions.DefaultOptions) : string.Empty,
            Reported = insight.Reported,
            Locations = insight.Locations?.Select(l => new InsightLocationEntity { LocationId = l }).ToList(),
            Tags = insight.Tags?.ToList()
		};
	}

    
    private static IEnumerable<Point> MapInsightPoints(string entityPointsJson)
    {
        return string.IsNullOrEmpty(entityPointsJson) ? null : JsonSerializer.Deserialize<IEnumerable<Point>>(entityPointsJson, JsonSerializerExtensions.DefaultOptions);
    }
    private static int PreviousStatusCount(InsightEntity entity, InsightStatus insightStatus)
	{
		var previousStatus = entity.StatusLogs?.Count(c => c.Status == insightStatus) ?? 0;
		if (entity.Status ==insightStatus && previousStatus > 0)
			return previousStatus - 1;
		return previousStatus;
	}
}
