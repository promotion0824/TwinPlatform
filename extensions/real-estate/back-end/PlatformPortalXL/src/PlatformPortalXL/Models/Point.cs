using System;
using System.Collections.Generic;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;

namespace PlatformPortalXL.Models
{
    public class Point
    {
        public Guid Id { get; set; }
        public string TwinId { get; set; }

        public Guid EntityId { get; set; }

        public string Name { get; set; }

        public Guid CustomerId { get; set; }

        public Guid SiteId { get; set; }

        public string Unit { get; set; }

        public PointType Type { get; set; }

        public string ExternalPointId { get; set; }

        public Guid EquipmentId { get; set; }
        public Guid DeviceId { get; set; }

        public IList<Tag> Tags { get; set; }
        public IList<Equipment> Equipment { get; set; }

        public decimal? DisplayPriority { get; set; }
        public string DisplayName { get; set; }
        public Dictionary<string, DigitalTwinProperty> Properties { get; set; }
    }
}
