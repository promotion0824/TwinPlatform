using SiteCore.Domain;
using System.Collections.Generic;
using System.Linq;

namespace SiteCore.Dto
{
    public class MetricDto
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Tooltip { get; set; }
        public MetricColor? Color { get; set; }
        public List<MetricDto> Metrics { get; set; }

        public static MetricDto MapFrom(Metric model) => new MetricDto
        {
            Key = model.Key,
            Name = model.Name,
            Value = model.FormattedAverage,
            Tooltip = model.Tooltip,
            Color = MapStatusToColor(model.Status),
            Metrics = MapFrom(model.Metrics)
        };

        private static MetricColor? MapStatusToColor(MetricStatus? status)
        {
            switch (status)
            {
                case MetricStatus.Ok:
                    return MetricColor.Green;
                case MetricStatus.Warning:
                    return MetricColor.Yellow;
                case MetricStatus.Error:
                    return MetricColor.Red;
                default:
                    return null;
            }
        }

        public static List<MetricDto> MapFrom(List<Metric> models)
        {
            if (models == null || !models.Any())
            {
                return null;
            }
            return models?.Select(MapFrom).ToList();
        }
    }

    public enum MetricColor
    {
        Green,
        Yellow,
        Red
    }
}
