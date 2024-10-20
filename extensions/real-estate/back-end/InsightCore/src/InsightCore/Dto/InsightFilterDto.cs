using System;
using System.Collections.Generic;

namespace InsightCore.Dto;

public class InsightFilterDto
{
    public List<Guid> SiteIds { get; set; }
    public Dictionary<string, List<string>> Filters { get; set; }
}
public enum InsightFilterType
{
    DetailedStatus,
    InsightType,
    PrimaryModelId,
    Activity,
    SourceNames
}
public enum InsightActivityType
{
    Tickets,
    PreviouslyResolved,
    PreviouslyIgnored,
    Reported
}
public static class InsightFilterNames
{
    public static string DetailedStatus = "detailedStatus";
    public static string InsightTypes = "insightTypes";
    public static string PrimaryModelIds = "primaryModelIds";
    public static string Activity = "activity";
    public static string SourceNames = "sourceNames";
}
