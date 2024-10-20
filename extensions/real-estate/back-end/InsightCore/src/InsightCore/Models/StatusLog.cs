using System;
using System.Collections.Generic;

namespace InsightCore.Models
{
	public class StatusLog
	{
		public Guid Id { get; set; }
		public Guid InsightId { get; set; }
		public Guid? UserId { get; set; }
		public SourceType? SourceType { get; set; }
		public Guid? SourceId { get; set; }
		public InsightStatus Status { get; set; }
		public DateTime CreatedDateTime { get; set; }
		public string Reason { get; set; }
		public int Priority { get; set; }
		public int OccurrenceCount { get; set; }
		public List<ImpactScore> ImpactScores { get; set; }

        public static StatusLog MapFrom(Insight insight, InsightStatusChangeDto dto)
        {
            return new StatusLog
            {
                Id = Guid.NewGuid(),
                CreatedDateTime = DateTime.UtcNow,
                InsightId = insight.Id,
                Priority = insight.Priority,
                OccurrenceCount = insight.OccurrenceCount,
                ImpactScores = insight.ImpactScores,
                Status = dto.Status.Value,
                SourceId = dto.SourceId,
                SourceType = dto.SourceId.HasValue ? Models.SourceType.App : Models.SourceType.Willow,
                UserId = dto.UserId,
                Reason = dto.Reason
            };
        }

	}
}
