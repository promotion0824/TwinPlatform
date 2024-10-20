using System;
using System.Collections.Generic;
using MobileXL.Models.Enums;

namespace MobileXL.Models
{
    public class Point
    {
        public Guid Id { get; set; }
        public Guid EntityId { get; set; }
        public string Name { get; set; }
        public Guid CustomerId { get; set; }
        public Guid SiteId { get; set; }
        public string Unit { get; set; }
        public PointType Type { get; set; }
        public string ExternalPointId { get; set; }
        public string Category { get; set; }
        public string Metadata { get; set; }
        public bool IsDetected { get; set; }
        public Guid DeviceId { get; set; }
        public bool IsEnabled { get; set; }
        public Guid EquipmentId { get; set; }
        public IList<Tag> Tags { get; set; }
        public IList<Equipment> Equipment { get; set; }
    }
}
