using PlatformPortalXL.Extensions;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatformPortalXL.Dto
{
    public class InsightSimpleDto
    {
        public Guid Id { get; set; }
        public Guid SiteId { get; set; }
        public string SequenceNumber { get; set; }
        public string FloorCode { get; set; }
        public Guid? EquipmentId { get; set; }
        public string TwinId { get; set; }
		public InsightType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }
        [Obsolete("Use 'LastStatus' instead")]
        public OldInsightStatus Status { get; set; }
        public InsightStatus LastStatus { get; set; }
        public InsightState State { get; set; }
        public InsightSourceType SourceType { get; set; }
        public DateTime OccurredDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string ExternalId { get; set; }
        public int OccurrenceCount { get; set; }
        public string SourceName { get; set; }
        public Guid? FloorId { get; set; }
        public List<ImpactScore> ImpactScores { get; set; }
		public string EquipmentName { get; set; }
		public string RuleId { get; set; }
		public string RuleName { get; set; }
		public string PrimaryModelId { get; set; }
		public int TicketCount { get; set; }
		public bool NewOccurrence { get; set; }
		public bool EcmDependency { get; set; }
		public int PreviouslyIgnored { get; set; }
		public int PreviouslyResolved { get; set; }
		public int PreviouslyResolvedAndIgnoredCount => PreviouslyIgnored + PreviouslyResolved;
        public DateTime? LastResolvedDate { get; set; }
        public DateTime? LastIgnoredDate { get; set; }
        public bool Reported { get; set; }
        public static InsightSimpleDto MapFromModel(Insight insight,bool ecmDependency)
        {
            return new InsightSimpleDto
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
                Description = insight.Description,
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
                Reported = insight.Reported
            };
        }

        public static List<InsightSimpleDto> MapFromModels(List<Insight> insights)
        {
	        return insights?.Select(insight => MapFromModel(insight, insight.GetEcmDependency(insights))).ToList();

		}
      
	}
}
