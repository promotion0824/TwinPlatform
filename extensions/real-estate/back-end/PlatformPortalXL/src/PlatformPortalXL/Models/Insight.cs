using System;
using System.Collections.Generic;

namespace PlatformPortalXL.Models
{
    public class Insight
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid SiteId { get; set; }
        public string SequenceNumber { get; set; }
        public string FloorCode { get; set; }
        public Guid? EquipmentId { get; set; }
        public string TwinId { get; set; }
        public string TwinName { get; set; }
		public InsightType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Recommendation { get; set; }
        public List<ImpactScore> ImpactScores { get; set; }
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
		public Guid? CreatedUserId { get; set; }
		public string RuleId { get; set; }
		public string RuleName { get; set; }
		public string PrimaryModelId { get; set; }
		public bool NewOccurrence { get; set; }
		public int PreviouslyIgnored { get; set; }
		public int PreviouslyResolved { get; set; }
		public Guid? FloorId { get; set; }
        public DateTime? LastResolvedDate { get; set; }
        public DateTime? LastIgnoredDate { get; set; }
        public bool Reported { get; set; }
        /// <summary>
        /// The scopeId is a dtId of a twin which could be a portfolio, a campus or a site.
        /// </summary>
        public string ScopeId { get; set; }
    }
}
