using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;

namespace PlatformPortalXL.Dto;

public class InsightDiagnosticDto
{
    public Guid Id { get; set; }
    public Guid SiteId { get; set; }
    public string SequenceNumber { get; set; }
    public string TwinId { get; set; }
    public string TwinName { get; set; }
    public InsightType Type { get; set; }
    public int Priority { get; set; }
    public InsightStatus LastStatus { get; set; }
    public string PrimaryModelId { get; set; }
    public int OccurrenceCount { get; set; }
    public string RuleId { get; set; }
    public string RuleName { get; set; }
    public Guid ParentId { get; set; }
    public bool Check { get; set; }
    public OccurrenceLiveData OccurrenceLiveData { get; set; }
}
public class OccurrenceLiveData
{
    public Guid PointId { get; set; }
    public Guid PointEntityId { get; set; }
    public string PointName { get; set; }
    public string PointType => "Binary";
    public string Unit => "Bool";
    public List<TimeSeriesBinaryData> TimeSeriesData { get; set; }
}
public class TimeSeriesBinaryData
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public bool IsFaulty { get; set; }
}
