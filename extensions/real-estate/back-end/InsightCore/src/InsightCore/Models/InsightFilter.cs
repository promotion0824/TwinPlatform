using System.Collections.Generic;
using System;

namespace InsightCore.Models;

public class InsightFilter
{
    public Guid Id { get; set; }
    public Guid SiteId { get; set; }
    public InsightType Type { get; set; }
    public string Name { get; set; }
    public InsightStatus Status { get; set; }
    public SourceType SourceType { get; set; }
    public Guid? SourceId { get; set; }
    public string PrimaryModelId { get; set; }
    public bool Reported { get; set; }
    public IEnumerable<InsightStatus> StatusLogs { get; set; }

}

public class InsightFilterSourceData
{
    public string SourceName { get; set; }
    public Guid? SourceId { get; set; }

}
