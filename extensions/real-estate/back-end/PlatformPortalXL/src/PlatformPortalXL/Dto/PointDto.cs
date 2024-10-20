using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using static PlatformPortalXL.ServicesApi.DigitalTwinApi.DigitalTwinPoint;

namespace PlatformPortalXL.Dto
{
    public class PointDto
    {
        public string ModelId { get; set; }
        public string TwinId { get; set; }
        public Guid Id { get; set; }
        public Guid TrendId { get; set; }
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<TagDto> Tags { get; set; }
        public PointType Type { get; set; }
        public PointValue CurrentValue { get; set; }
        public decimal? DisplayPriority { get; set; }
        public string DisplayName { get; set; }
        public List<PointAssetDto> Assets { get; set; }
        public Dictionary<string, Property> Properties { get; set; }
        public bool? IsEnabled { get; set; }
        public bool? IsDetected { get; set; }
        public Guid? DeviceId { get; set; }
        public DeviceDto Device { get; set; }
        public string TrendInterval { get; set; }
        public string CategoryName { get; set; }
        public string ConnectorName { get; set; }
    }
}
