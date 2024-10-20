using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PlatformPortalXL.Models
{
    public class Equipment
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid CustomerId { get; set; }

        public Guid SiteId { get; set; }

        public Guid? FloorId { get; set; }

        public string ExternalEquipmentId { get; set; }

        public string Category { get; set; }

        public Guid? ParentEquipmentId { get; set; }

        public List<Point> Points { get; set; }

        public List<Tag> Tags { get; set; } = new List<Tag>();

        public List<Tag> PointTags { get; set; } = new List<Tag>();

        public List<Guid> CategoryIds { get; set; } = new List<Guid>();
    }
}
