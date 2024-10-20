using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using InsightCore.Models;

namespace InsightCore.Controllers.Requests
{
    public class UpdateInsightRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Recommendation { get; set; }
        public List<ImpactScore> ImpactScores { get; set; }
        public int? Priority { get; set; }
        public InsightType? Type { get; set; }
		[Obsolete("Use the LastStatus, this enum is not in used")]
        [EnumDataType(typeof(OldInsightStatus))]
        public OldInsightStatus? Status { get; set; }
        [EnumDataType(typeof(InsightStatus))]
        public InsightStatus? LastStatus { get; set; }
		[EnumDataType(typeof(InsightState))]
        public InsightState? State { get; set; }
        public DateTime? OccurredDate { get; set; }
        public DateTime? DetectedDate { get; set; }
        public string ExternalId { get; set; }
        public string ExternalStatus { get; set; }
        public string ExternalMetadata { get; set; }
        public int OccurrenceCount { get; set; }
		public string PrimaryModelId { get; set; }
        public Guid? UpdatedByUserId { get; set; }
        public Guid? SourceId { get; set; }
		public string RuleName { get; set; }
		public IEnumerable<InsightOccurrence> InsightOccurrences { get; set; }
		public string Reason { get; set; }
        public bool? Reported { get; set; }
        public IEnumerable<Dependency> Dependencies { get; set; }
        public IEnumerable<Point> Points { get; set; }
        public IEnumerable<string> Locations { get; set; }
        public IEnumerable<string> Tags { get; set; }
    }
}
