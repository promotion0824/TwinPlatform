using System;
using System.Collections.Generic;

namespace PlatformPortalXL.Dto;
public class InsightPointsDto
{
    public InsightPointsDto()
    {
        InsightPoints = new List<PointTwinDto>();
        ImpactScorePoints = new List<ImpactScorePointDto>();
    }
    public List<PointTwinDto> InsightPoints { get; set; }
    public List<ImpactScorePointDto> ImpactScorePoints { get; set; }

    public class PointTwinDto
    {
        public string PointTwinId { get; set; }
        public Guid TrendId { get; set; }
        public string Name { get; set; }
        public string ExternalId { get; set; }
        public string Unit { get; set; }
    }
    public class ImpactScorePointDto
    {
        public string Name { get; set; }
        public string ExternalId { get; set; }
        public string Unit { get; set; }
    }

}

