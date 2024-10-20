using System;

namespace MobileXL.Models
{
    public class Insight
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid SiteId { get; set; }
        public string FloorCode { get; set; }
        public Guid? EquipmentId { get; set; }
        public InsightType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }
        public InsightStatus LastStatus { get; set; }
        public DateTime OccurredDate { get; set; }
        public DateTime DetectedDate { get; set; }
        public InsightSourceType SourceType { get; set; }
        public Guid? SourceId { get; set; }
        public string RuleId { get; set; }
		public string RuleName { get; set; }
		public string PrimaryModelId { get; set; }
		public string TwinId { get; set; }
	}
}
