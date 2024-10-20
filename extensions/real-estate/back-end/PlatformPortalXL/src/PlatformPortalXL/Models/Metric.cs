using System.Collections.Generic;

namespace PlatformPortalXL.Models
{
    public class Metric
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Tooltip { get; set; }
        public MetricColor? Color { get; set; }
        public List<Metric> Metrics { get; set; }
    }

    public enum MetricColor
    {
        Green,
        Yellow,
        Red
    }
}
