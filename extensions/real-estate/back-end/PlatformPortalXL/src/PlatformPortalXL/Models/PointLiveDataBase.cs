using System;

namespace PlatformPortalXL.Models
{
    public class PointLiveDataBase
    {
        public Guid PointId { get; set; }
        public Guid PointEntityId { get; set; }
        public string PointName { get; set; }
        public PointType PointType { get; set; }
        public string Unit { get; set; }
        public decimal? DisplayPriority { get; set; }
    }
}
