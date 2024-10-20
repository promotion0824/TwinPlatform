using InsightCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using InsightCore.Infrastructure.Extensions;
using InsightCore.Services;

namespace InsightCore.Dto
{
    public class InsightDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid SiteId { get; set; }
        public string SequenceNumber { get; set; }
        public Guid? EquipmentId { get; set; }
        public string TwinId { get; set; }
        public string TwinName { get; set; }
		public InsightType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Recommendation { get; set; }
        public List<ImpactScore> ImpactScores { get; set; }
        public int Priority { get; set; }
        public OldInsightStatus Status { get; set; }
        public InsightStatus LastStatus { get; set; }
		public InsightState State { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public DateTime OccurredDate { get; set; }
        public DateTime DetectedDate { get; set; }
        public SourceType SourceType { get; set; }
        public Guid? SourceId { get; set; }
        public string SourceName { get; set; }
        public string PrimaryModelId { get; set; }
        public string ExternalId { get; set; }
        public string ExternalStatus { get; set; }
        public string ExternalMetadata { get; set; }
		public int OccurrenceCount { get; set; }
		public Guid? CreatedUserId { get; set; }
		public string RuleId { get; set; }
		public string RuleName { get; set; }
		public bool NewOccurrence { get; set; }
		public int PreviouslyIgnored { get; set; }
		public int PreviouslyResolved { get; set; }
		public Guid? FloorId { get; set; }
        public bool Reported { get; set; }
        public DateTime? LastResolvedDate { get; set; }
        public DateTime? LastIgnoredDate { get; set; }
        public IEnumerable<string> Locations { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public static InsightDto MapFrom(Insight insight,string sourceName)
		{
			if (insight == null)
				return null;

            if (insight.ImpactScores is not null &&  insight.ImpactScores.Any(x => ImpactScore.Priority.Contains(x.FieldId)))
            {
                var impactScorePriority = insight.ImpactScores.FirstOrDefault(x => ImpactScore.Priority.Contains(x.FieldId)).Value;
                insight.Priority = InsightRepository.ConvertPriority(impactScorePriority);
            }

            return new InsightDto
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
                ImpactScores = insight.ImpactScores ?? [],
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
                SourceName =insight.SourceType==SourceType.App?sourceName:$"{insight.SourceType}",
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
                Locations = insight.Locations,
                Tags = insight.Tags

            };
        }

    }
}
