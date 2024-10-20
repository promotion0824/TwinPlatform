using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using InsightCore.Models;

namespace InsightCore.Controllers.Requests
{
    public class AppCreateInsightRequest
    {
        public Guid CustomerId { get; set; }
        public string SequenceNumberPrefix { get; set; }
        [EnumDataType(typeof(InsightType))]
        public InsightType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Recommendation { get; set; }
        public List<ImpactScore> ImpactScores { get; set; }
        public int Priority { get; set; }
        [EnumDataType(typeof(OldInsightStatus))]
        public OldInsightStatus Status { get; set; }
        [EnumDataType(typeof(InsightState))]
        public InsightState State { get; set; }
        public DateTime OccurredDate { get; set; }
        public DateTime DetectedDate { get; set; }
        public string ExternalId { get; set; }
        public string ExternalStatus { get; set; }
        public string ExternalMetadata { get; set; }
        public int OccurrenceCount { get; set; }
        public Dictionary<string, string> AnalyticsProperties { get; set; }
		[Required]
		public string TwinId { get; set; }
		public string RuleId { get; set; }
		public string RuleName { get; set; }
		public string PrimaryModelId { get; set; }
		public IEnumerable<InsightOccurrence> InsightOccurrences { get; set; }
		public IEnumerable<Dependency> Dependencies { get; set; }
        public IEnumerable<Point> Points { get; set; }
        public IEnumerable<string> Locations { get; set; }
        public IEnumerable<string> Tags { get; set; }
}
}
