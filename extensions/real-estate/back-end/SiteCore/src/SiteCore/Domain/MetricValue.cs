using SiteCore.Entities;
using System;

namespace SiteCore.Domain
{
    public class MetricValue
    {
        public decimal Value { get; set; }
        public DateTime TimeStamp { get; set; }

        public static MetricValue MapFrom(SiteMetricValueEntity entity) => 
            new MetricValue
            {
                TimeStamp = entity.TimeStamp,
                Value = entity.Value
            };
    }
}
