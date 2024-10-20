using DigitalTwinCore.Models.Connectors;
using System;
using System.Collections.Generic;

namespace DigitalTwinCore.Models
{
    public class Point
    {
        public string ModelId { get; set; }
        public string TwinId { get; set; }
        public Guid Id { get; set; }

        public Guid TrendId { get; set; }
        public TimeSpan? TrendInterval { get; set; }

        public string ExternalId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public PointType Type { get; set; }

        public string CategoryName { get; set; }

        public List<Tag> Tags { get; set; }
        public Dictionary<string, Property> Properties { get; set; }
        public decimal? DisplayPriority { get; set; }
        public string DisplayName { get; set; }

        public List<Twin> Assets { get; set; }
        public List<Twin> Devices { get; set; }
        public PointCommunication Communication { get; set; }

        public bool? IsEnabled { get; set; }
        public bool? IsDetected { get; set; }

        public PointValue CurrentValue { get; set; }
    }

    public enum PointType
    {
        Undefined = 0,
        Analog = 1,
        Binary = 2,
        MultiState = 3, // Ivestigate: This may be able to be removed
        Sum = 5
    }

    public class PointValue
    {
        public string Unit { get; set; }
        public object Value { get; set; }
    }

    public class PointTwinDto
    {
        public string PointTwinId { get; set; }
        public Guid TrendId { get; set; }
        public string Name { get; set; }
        public string ExternalId { get; set; }
        public string Unit { get; set; }
    }
}
