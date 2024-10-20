using InsightCore.Models;
using System.Collections.Generic;
using System.Linq;
using InsightCore.Infrastructure.Extensions;

namespace InsightCore.Dto
{
    public class InsightMapViewDto:InsightDto
    {
        public IEnumerable<InsightOccurrenceDto> Occurrences { get; set; }

		public new static InsightMapViewDto MapFrom(Insight insight, string sourceName)
		{
			if (insight == null)
				return null;
            return new InsightMapViewDto
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
                Description = insight.Description,
                Recommendation = insight.Recommendation,
                ImpactScores = insight.ImpactScores,
                Priority = insight.Priority,
                Status = insight.Status.Convert(),
				LastStatus = insight.Status,
                State = insight.State,
                CreatedDate = insight.CreatedDate,
                UpdatedDate = insight.UpdatedDate,
                OccurredDate = insight.LastOccurredDate,
                DetectedDate = insight.DetectedDate,
                SourceType = insight.SourceType,
                SourceId = insight.SourceId,
                SourceName = insight.SourceType == SourceType.App ? sourceName : $"{insight.SourceType}",
				PrimaryModelId = insight.PrimaryModelId,
                ExternalId = insight.ExternalId,
                ExternalStatus = insight.ExternalStatus,
                ExternalMetadata = insight.ExternalMetadata,
                OccurrenceCount = insight.OccurrenceCount,
				CreatedUserId= insight.CreatedUserId,
				RuleId= insight.RuleId,
				RuleName= insight.RuleName,
				NewOccurrence = insight.NewOccurrence,
				PreviouslyIgnored = insight.PreviouslyIgnored,
				PreviouslyResolved = insight.PreviouslyResolved,
				FloorId = insight.FloorId,
                LastResolvedDate = insight.LastResolvedDate,
                LastIgnoredDate = insight.LastIgnoredDate,
                Reported = insight.Reported,
                Occurrences = InsightOccurrenceDto.MapFrom(insight.InsightOccurrences)

            };
        }

    }
}
