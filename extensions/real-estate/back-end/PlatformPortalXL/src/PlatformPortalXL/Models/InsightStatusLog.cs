using System;
using System.Collections.Generic;

namespace PlatformPortalXL.Models;

public class InsightStatusLog
{
	public Guid Id { get; set; }
	public Guid InsightId { get; set; }
	public InsightSourceType? SourceType { get; set; }
	public Guid? UserId { get; set; }
	public Guid? SourceId { get; set; }
    public string SourceName { get; set; }
	public InsightStatus Status { get; set; }
	public DateTime CreatedDateTime { get; set; }
	public string Reason { get; set; }
	public int Priority { get; set; }
	public int OccurrenceCount { get; set; }
	public List<ImpactScore> ImpactScores { get; set; }
	public bool PreviouslyIgnored { get; set; }
	public bool PreviouslyResolved { get; set; }
}

