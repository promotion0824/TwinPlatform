using PlatformPortalXL.Extensions;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.ServicesApi.InsightApi;

namespace PlatformPortalXL.Dto
{
    public class InsightMapViewDto:InsightSimpleDto
    {
        public IEnumerable<InsightOccurrenceDto> Occurrences { get; set; }
        public static InsightMapViewDto MapFromModel(InsightMapViewResponse insight,bool ecmDependency)
        {
            return new InsightMapViewDto
            {
                Id = insight.Id,
                SiteId = insight.SiteId,
                SequenceNumber = insight.SequenceNumber,
                FloorCode = insight.FloorCode,
                EquipmentId = insight.EquipmentId,
                TwinId = insight.TwinId,
				EquipmentName = insight.TwinName,
				Type = insight.Type,
                Name = insight.Name,
                Priority = insight.Priority,
                Status = insight.Status,
                LastStatus = insight.LastStatus,
                State = insight.State,
                SourceType = insight.SourceType,
                SourceName = insight.SourceName,
                OccurredDate = insight.OccurredDate,
                UpdatedDate = insight.UpdatedDate,
                ExternalId = insight.ExternalId,
                OccurrenceCount = insight.OccurrenceCount,      
                FloorId = insight.FloorId, 
                ImpactScores = insight.ImpactScores,
				RuleId= insight.RuleId,
				RuleName= insight.RuleName,
				PrimaryModelId = insight.PrimaryModelId,
				NewOccurrence = insight.NewOccurrence,
				EcmDependency = ecmDependency,
				PreviouslyIgnored = insight.PreviouslyIgnored,
				PreviouslyResolved = insight.PreviouslyResolved,
                LastIgnoredDate = insight.LastIgnoredDate,
                LastResolvedDate = insight.LastResolvedDate,
                Reported = insight.Reported,
                Occurrences = InsightOccurrenceDto.MapFromModels(insight.Occurrences,insight.Id)
            };
        }

        public static List<InsightMapViewDto> MapFromModels(List<InsightMapViewResponse> insights)
        {
	        return insights?.Select(insight => MapFromModel(insight, insight.GetEcmDependency(insights))).ToList();

		}
      
	}
}
