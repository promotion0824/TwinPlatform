using PlatformPortalXL.Models;
using System.Collections.Generic;
using System.Linq;

namespace PlatformPortalXL.Dto
{
    public class MetricDto
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Tooltip { get; set; }
        public MetricColor? Color { get; set; }
        public List<MetricDto> Metrics { get; set; }

        internal static MetricDto MapFrom(Metric model) => new MetricDto
        {
            Key = model.Key,
            Name = model.Name,
            Value = model.Value,
            Tooltip = model.Tooltip,
            Color = model.Color,
            Metrics = MapFrom(model.Metrics)
        };

        internal static List<MetricDto> MapFrom(List<Metric> models)
        {
            return models?.Select(MapFrom).ToList();
        }
    }
}
