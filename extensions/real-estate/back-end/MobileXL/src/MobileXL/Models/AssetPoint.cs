using MobileXL.Models.Enums;
using System;
using System.Collections.Generic;

namespace MobileXL.Models
{
    public class AssetPoint
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public PointType Type { get; set; }
        public string ExternalPointId { get; set; }
        public IList<Tag> Tags { get; set; }
    }
}
