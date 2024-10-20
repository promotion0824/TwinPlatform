using System;
using System.Collections.Generic;

namespace Willow.IoTService.Monitoring.Dtos.LiveDataCore;

public class UniqueTrendsResult
{
    public IEnumerable<UniqueTrendsDto>? Data { get; init; }
}
public class UniqueTrendsDto
{
        public Guid ConnectorId { get; set; }
        public int TotalCapabilities { get; set; }
        public int ActiveCapabilities { get; set; }
        public int InactiveCapabilities { get; set; }
        public int TrendingCapabilities { get; set; }
}