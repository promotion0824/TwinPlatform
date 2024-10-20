using SiteCore.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiteCore.Dto
{
    public class SiteMetricsDto
    {
        public Guid SiteId { get; set; }
        public List<MetricDto> Metrics { get; set; }

        public static SiteMetricsDto MapFrom(SiteMetrics model) => 
            new SiteMetricsDto
            {
                SiteId = model.SiteId,
                Metrics = MetricDto.MapFrom(model.Metrics)
            };

        public static List<SiteMetricsDto> MapFrom(List<SiteMetrics> models) => 
            models?.Select(MapFrom).ToList() ?? new List<SiteMetricsDto>();
    }
}
