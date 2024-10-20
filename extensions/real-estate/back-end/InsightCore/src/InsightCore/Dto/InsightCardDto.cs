using InsightCore.Models;
using InsightCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InsightCore.Dto
{
    public class InsightCardDto
    {
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public InsightType? InsightType { get; set; }  // insighttype - one per ruleid
        public int Priority { get; set; }
        public Guid? SourceId { get; set; }
        public string SourceName { get; set; }
        public string PrimaryModelId { get; set; }
        public string Recommendation { get; set; }
        public int InsightCount { get; set; }
        public DateTime LastOccurredDate { get; set; }
        public List<ImpactScore> ImpactScores { get; set; }
        public static InsightCardDto MapFrom(InsightCard insightCard,string sourceName)
		{
			if (insightCard == null)
				return null;

            var insightCardDto = new InsightCardDto
            {
                 InsightCount = insightCard.InsightCount,
                 ImpactScores = insightCard.ImpactScores,
                 InsightType = insightCard.InsightType,
                 LastOccurredDate = insightCard.LastOccurredDate,
                 PrimaryModelId = insightCard.PrimaryModelId,
                 Priority = insightCard.Priority,
                 Recommendation = insightCard.Recommendation,
                 RuleId = insightCard.RuleId,
                 RuleName = insightCard.RuleName,
                 SourceId = insightCard.SourceId,
                 SourceName = insightCard.SourceId.HasValue ? sourceName :null,
            };

            if (insightCard.ImpactScores is not null && insightCard.ImpactScores.Any(x => ImpactScore.Priority.Contains(x.FieldId)))
            {
                var impactScorePriority = insightCard.ImpactScores.FirstOrDefault(x => ImpactScore.Priority.Contains(x.FieldId)).Value;
                insightCardDto.Priority = InsightRepository.ConvertPriority(impactScorePriority); 
            }

            return insightCardDto;
        }
         
    }
}
