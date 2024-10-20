using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using Willow.Platform.Users;

namespace PlatformPortalXL.Dto
{
    public class InsightDetailDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid SiteId { get; set; }
        public string SequenceNumber { get; set; }
        public string FloorCode { get; set; }
        public Guid? EquipmentId { get; set; }
        public string TwinId { get; set; }
		public Guid? FloorId { get; set; }
        public InsightType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }
        [Obsolete("Use 'LastStatus' instead")]
        public OldInsightStatus Status { get; set; }
        public InsightStatus LastStatus { get; set; }
        public InsightState State { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public DateTime OccurredDate { get; set; }
        public DateTime DetectedDate { get; set; }
        public InsightSourceType SourceType { get; set; }
        public Guid? SourceId { get; set; }
        public string SourceName { get; set; }
        public string ExternalId { get; set; }
        public string ExternalStatus { get; set; }
        public string ExternalMetadata { get; set; }
        public int OccurrenceCount { get; set; }
        public List<ImpactScore> ImpactScores { get; set; }
        public string Recommendation { get; set; }
		public User CreatedUser { get; set; }
        public string EquipmentName { get; set; }
		public string RuleId { get; set; }
		public string RuleName { get; set; }
		public string PrimaryModelId { get; set; }
		public bool NewOccurrence { get; set; }
		public int PreviouslyIgnored { get; set; }
		public int PreviouslyResolved { get; set; }
        public DateTime? LastResolvedDate { get; set; }
        public DateTime? LastIgnoredDate { get; set; }
        public bool Reported { get; set; }
        public int PreviouslyResolvedAndIgnoredCount => PreviouslyIgnored + PreviouslyResolved;

        public static InsightDetailDto MapFromModel(Insight insight)
		{
			if (insight == null)
				return null;
            return new InsightDetailDto
            {
                Id = insight.Id,
                CustomerId = insight.CustomerId,
                SiteId = insight.SiteId,
                SequenceNumber = insight.SequenceNumber,
                FloorCode = insight.FloorCode,
                EquipmentId = insight.EquipmentId,
                TwinId = insight.TwinId,
				EquipmentName = insight.TwinName,
				Type = insight.Type,
                Name = insight.Name,
                Description = insight.Description,
                Recommendation = insight.Recommendation,
                ImpactScores = insight.ImpactScores,
                Priority = insight.Priority,
                Status = insight.Status,
                LastStatus = insight.LastStatus,
                State = insight.State,
                CreatedDate = insight.CreatedDate,
                UpdatedDate = insight.UpdatedDate,
                OccurredDate = insight.OccurredDate,
                DetectedDate = insight.DetectedDate,
                SourceType = insight.SourceType,
                SourceId = insight.SourceId,
                SourceName = insight.SourceName,
                ExternalId = insight.ExternalId,
                ExternalStatus = insight.ExternalStatus,
                ExternalMetadata = insight.ExternalMetadata,
                FloorId = insight.FloorId,
                OccurrenceCount = insight.OccurrenceCount,
				RuleId= insight.RuleId,
				RuleName=insight.RuleName,
				PrimaryModelId = insight.PrimaryModelId,
				NewOccurrence = insight.NewOccurrence,
				PreviouslyIgnored = insight.PreviouslyIgnored,
				PreviouslyResolved = insight.PreviouslyResolved,
                LastIgnoredDate = insight.LastIgnoredDate,
                LastResolvedDate = insight.LastResolvedDate,
                Reported = insight.Reported
            };
        }

    }
}
