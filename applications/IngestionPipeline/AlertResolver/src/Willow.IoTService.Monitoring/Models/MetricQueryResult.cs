using System.Collections.Generic;

namespace Willow.IoTService.Monitoring.Models
{
    public class MetricQueryResult
    {
        public string? Key { get; set; }

        public IEnumerable<IDictionary<string, object>> Results { get; set; } = new List<Dictionary<string, object>>();
    }
}